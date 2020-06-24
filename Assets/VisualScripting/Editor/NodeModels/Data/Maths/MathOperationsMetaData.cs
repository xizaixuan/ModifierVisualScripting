using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modifier.Runtime;
using Modifier.Runtime.Mathematics;
using Unity.Mathematics;
using UnityEngine.Assertions;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.NodeModels
{
    public static class MathOperationsMetaData
    {
        // warning: those values are serialized. don't mess with the order/values
        public enum CustomOps
        {
            None = 0,
            Add = 1,
            Subtract = 2,
            Multiply = 3,
            Divide = 4,
            Negate = 5,
            Modulo = 6,
            CubicRoot = 7
        }

        // warning: those values are serialized. don't mess with the order/values
        public enum MathOps
        {
            None = 0,
            Sin = 1,
            Cos = 2,
            Tan = 3,
            Sinh = 4,
            Cosh = 5,
            Tanh = 6,
            Asin = 7,
            Acos = 8,
            Atan = 9,
            Atan2 = 10,
            Round = 11,
            Ceil = 12,
            Floor = 13,
            Abs = 14,
            Exp = 15,
            Log10 = 16,
            Log2 = 17,
            Sign = 18,
            Sqrt = 19,
            Dot = 20,
            Cross = 21,
            Pow = 22,
            Min = 23,
            Max = 24,
            Normalize = 25,
            NormalizeSafe = 26,
            // Select = 27,
            Reflect = 28,
            Refract = 29,
            Clamp = 30,
            Remap = 31,
            Length = 32,
            Float2 = 33,
            Float3 = 34,
            Float4 = 35,
            Distance = 36,
        }

        static string[] s_MethodNamesWithMultipleInputs =
        {
            "add", "multiply", "min", "max"
        };

        public static bool MethodNameSupportsMultipleInputs(string methodName)
        {
            var m = methodName.ToLower();
            return s_MethodNamesWithMultipleInputs.Contains(m);
        }

        public struct OpSignature
        {
            public ValueType Return;
            public ValueType[] Params;
            // public string OpType;
            public CustomOps CustomOp;
            public MathOps MathOp;
            public string OpType;
            public ulong StableId;
            public bool IsCustomOp => CustomOp != CustomOps.None;

            public string EnumName => OpType + String.Join("", Params);

            public OpSignature(ValueType @return, CustomOps opType, params ValueType[] @params)
            {
                Params = @params;
                Return = @return;
                CustomOp = opType;
                MathOp = MathOps.None;
                OpType = opType.ToString();
                StableId = MathCodeGeneration.GenerateStableValueForCustomOp(opType, @return, @params);
            }

            public OpSignature(ValueType @return, MathOps opType, params ValueType[] @params)
            {
                Params = @params;
                Return = @return;
                CustomOp = CustomOps.None;
                MathOp = opType;
                OpType = opType.ToString();
                StableId = MathCodeGeneration.GenerateStableValueForMathOp(opType, @return, @params);
            }

            public static OpSignature LinearBinOp(CustomOps opType, ValueType valueType)
            {
                return new OpSignature(valueType, opType, valueType, valueType);
            }

            public override string ToString()
            {
                return $"{Return} {OpType}({String.Join(", ", Params.Select(p => p.ToString()))}) ({EnumName})";
            }

            public bool SupportsMultiInputs() => MethodNameSupportsMultipleInputs(OpType);
        }

        static List<OpSignature> s_SupportedMethods;

        static List<OpSignature> s_SupportedMathMethods;

        static List<OpSignature> s_SupportedCustomMethods;

        static Dictionary<string, OpSignature[]> s_MethodsByName;

        static Dictionary<OpSignature, MathGeneratedFunction> s_EnumForSignature;
        static Dictionary<MathGeneratedFunction, OpSignature> s_SignatureForEnum;

        // = concat(SupportedMathMethods, SupportedCustomMethods)
        public static IReadOnlyList<OpSignature> SupportedMethods => s_SupportedMethods ?? (s_SupportedMethods = GetSupportedMethods());

        public static IReadOnlyList<OpSignature> SupportedMathMethods => s_SupportedMathMethods ?? (s_SupportedMathMethods = GetMathMethods());

        public static IReadOnlyList<OpSignature> SupportedCustomMethods => s_SupportedCustomMethods ?? (s_SupportedCustomMethods = GetCustomMethods());
        public static IReadOnlyDictionary<string, OpSignature[]> MethodsByName => s_MethodsByName ?? (s_MethodsByName = GetMethodsByName());
        public static IReadOnlyDictionary<OpSignature, MathGeneratedFunction> EnumForSignature => s_EnumForSignature ?? (s_EnumForSignature = GetEnumsForSignatures());
        public static IReadOnlyDictionary<MathGeneratedFunction, OpSignature> SignatureForEnum => s_SignatureForEnum ?? (s_SignatureForEnum = GetSignatureForEnum());

        static Dictionary<string, OpSignature[]> GetMethodsByName()
        {
            var opsByName = new Dictionary<string, List<OpSignature>>();
            foreach (var signature in SupportedMethods)
            {
                if (!opsByName.ContainsKey(signature.OpType))
                    opsByName.Add(signature.OpType, new List<OpSignature>());
                opsByName[signature.OpType].Add(signature);
            }

            foreach (var list in opsByName.Values)
            {
                ArrangeFloatParamsFirst(list);
            }

            return opsByName.ToDictionary(kp => kp.Key, kp => kp.Value.ToArray());
        }

        static void ArrangeFloatParamsFirst(List<OpSignature> list)
        {
            for (var i = 1; i < list.Count; i++)
            {
                var signature = list[i];
                if (!signature.Params.All(p => p == ValueType.Float))
                    continue;
                if (i == 0)
                    return;
                var tmp = list[0];
                list[0] = list[i];
                list[i] = tmp;
                return;
            }
        }

        static List<OpSignature> GetSupportedMethods()
        {
            var supportedMethods = SupportedMathMethods.Concat(SupportedCustomMethods).ToList();
            Assert.AreEqual(supportedMethods.Select(x => x.StableId).Distinct().Count(), supportedMethods.Count);
            return supportedMethods;
        }

        static List<OpSignature> GetMathMethods()
        {
            var funcNames = Enum.GetValues(typeof(MathOps)).Cast<MathOps>().ToDictionary(s => s.ToString().ToLower());
            var mathMethods = typeof(math).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var res = new List<OpSignature>(mathMethods.Length);
            foreach (var m in mathMethods)
            {
                if (funcNames.TryGetValue(m.Name, out var op))
                {
                    ValueType returnType = TypeToValueType(m.ReturnType);
                    if (returnType != ValueType.Unknown)
                    {
                        var paramTypes = m.GetParameters().Select(p => TypeToValueType(p.ParameterType)).ToArray();
                        if (paramTypes.All(p => p != ValueType.Unknown))
                        {
                            res.Add(new OpSignature(returnType, op, paramTypes));
                        }
                    }
                }
            }
            return res;
        }

        static List<OpSignature> GetCustomMethods()
        {
            var res = new List<OpSignature>();

            // types for which Op(T, T) is typed T, e.g. int add(int, int)
            var regularBinOps = new Dictionary<CustomOps, ValueType[]>
            {
                { CustomOps.Add, new[] { ValueType.Int, ValueType.Float, ValueType.Float2, ValueType.Float3, ValueType.Float4 } },
                { CustomOps.Subtract, new[] { ValueType.Int, ValueType.Float, ValueType.Float2, ValueType.Float3, ValueType.Float4 } },
                { CustomOps.Divide, new[] { ValueType.Int, ValueType.Float } },
                { CustomOps.Multiply, new[] { ValueType.Int, ValueType.Float, ValueType.Float2, ValueType.Float3, ValueType.Float4 } },
                { CustomOps.Modulo, new[] { ValueType.Int, ValueType.Float } },
            };

            foreach (var kp in regularBinOps)
            {
                foreach (var valueType in kp.Value)
                {
                    res.Add(OpSignature.LinearBinOp(kp.Key, valueType));
                }
            }

            var regularUnaryOps = new Dictionary<CustomOps, ValueType[]>
            {
                { CustomOps.Negate, new[] { ValueType.Float, ValueType.Float2, ValueType.Float3, ValueType.Float4, ValueType.Int } },
                { CustomOps.CubicRoot, new[] { ValueType.Float } },
            };

            foreach (var kp in regularUnaryOps)
            {
                foreach (var valueType in kp.Value)
                {
                    res.Add(new OpSignature(valueType, kp.Key, valueType));
                }
            }

            return res;
        }

        static Dictionary<OpSignature, MathGeneratedFunction> GetEnumsForSignatures()
        {
            return SupportedMethods.ToDictionary(o => o, o => Enum.TryParse(o.EnumName, out MathGeneratedFunction en) ? en : MathGeneratedFunction.NumMathFunctions);
        }

        static Dictionary<MathGeneratedFunction, OpSignature> GetSignatureForEnum()
        {
            return SupportedMethods.ToDictionary(o => Enum.TryParse(o.EnumName, out MathGeneratedFunction en) ? en : MathGeneratedFunction.NumMathFunctions, o => o);
        }

        public static OpSignature GetMethodsSignature(this MathGeneratedFunction function)
        {
            return SignatureForEnum.TryGetValue(function, out var res) ? res : default;
        }

        static ValueType TypeToValueType(Type t)
        {
            if (t == typeof(int))
                return ValueType.Int;
            if (t == typeof(bool))
                return ValueType.Bool;
            if (t == typeof(float))
                return ValueType.Float;
            if (t == typeof(float2))
                return ValueType.Float2;
            if (t == typeof(float3))
                return ValueType.Float3;
            if (t == typeof(float4))
                return ValueType.Float4;
            return ValueType.Unknown;
        }

        // Darren https://stackoverflow.com/a/27073919
        static string Capitalize(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            char[] a = s.ToCharArray();
            a[0] = Char.ToUpper(a[0]);
            return new string(a);
        }

        public static IOrderedEnumerable<(int Score, MathOperationsMetaData.OpSignature Signature)> ScoreCompatibleMethodsAccordingToInputParameters(MathOperationsMetaData.OpSignature[] methods, IEnumerable<ValueType> connectedInputTypes)
        {
            return methods.Select<OpSignature, (int Score, OpSignature Signature)>(s => SignatureScore(s, connectedInputTypes)).Where(s => s.Score > 0).OrderByDescending(s => s.Score);
        }

        public static (int Score, MathOperationsMetaData.OpSignature Signature) SignatureScore(
            MathOperationsMetaData.OpSignature signature, IEnumerable<ValueType> connectedInputTypes)
        {
            /* if the provided types are F3,F3,F:
             * F(F3,F3,F) should have the highest score
             * F(F3,F3,F3) is compatible (F to F3 is a valid cast)
             * F(F3,F3,bool) is clearly incompatible. score 0
             * for varargs methods (eg. add) we assume the last parameter of the method (usually the second) provides the type for extra args
             */
            int score = 0;
            int i = 0;
            var variadic = MethodNameSupportsMultipleInputs(signature.OpType);
            foreach (var providedType in connectedInputTypes)
            {
                int factor = 2;
                var paramType = signature.Params[math.min(i++, signature.Params.Length - 1)];
                if (i >= signature.Params.Length && !variadic)
                    factor = 1; // mismatched param count, impacts scores, but do not discard completely that in order to provide useful suggestion

                if (paramType == providedType)
                    score += 50 * factor;
                else if (providedType == ValueType.Unknown) // not connected yet ?
                    score += factor;
                else if (Value.CanConvert(providedType, paramType, false))
                    score += 5 * factor;
                else
                {
                    score = 0;
                    break;
                }
            }

            return (Score : score, Signature : signature);
        }

        public static bool GetMethodForOpAndArgumentTypes(string op, out MathGeneratedFunction result, out OpSignature resultSignature, IEnumerable<ValueType> types)
        {
            var methods = MethodsByName[op.Capitalize()];
            var bests = ScoreCompatibleMethodsAccordingToInputParameters(methods, types);
            var best = bests.FirstOrDefault();
            if (best.Score != 0)
            {
                result = (MathGeneratedFunction)Enum.Parse(typeof(MathGeneratedFunction), best.Signature.EnumName);
                resultSignature = best.Signature;
                return true;
            }

            result = default;
            resultSignature = default;
            return false;
        }
    }
}
