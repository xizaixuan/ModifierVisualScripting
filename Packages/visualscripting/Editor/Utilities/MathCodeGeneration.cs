using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Assert = Unity.Assertions.Assert;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.NodeModels
{
    static class MathCodeGeneration
    {
        private const uint k_IsCustomOpBit = 1u << 31;
        static string FileName => "MathematicsFunctions.gen.cs";

        static string EnumName => nameof(Modifier.Runtime.Mathematics.MathGeneratedFunction);
        static string ClassName => "MathGeneratedDelegates";
        static string LastEnumName => "NumMathFunctions";

        static string GetOpCodeFormat(MathOperationsMetaData.OpSignature sig)
        {
            switch (sig.OpType)
            {
                case nameof(MathOperationsMetaData.CustomOps.Negate):
                    return "- {0}";
                case nameof(MathOperationsMetaData.CustomOps.Modulo):
                    return "{0} % {1}";
                case nameof(MathOperationsMetaData.CustomOps.Add):
                    return "{0} + {1}";
                case nameof(MathOperationsMetaData.CustomOps.Subtract):
                    return "{0} - {1}";
                case nameof(MathOperationsMetaData.CustomOps.Multiply):
                    return "{0} * {1}";
                case nameof(MathOperationsMetaData.CustomOps.Divide):
                    return "{0} / {1}";
                case nameof(MathOperationsMetaData.CustomOps.CubicRoot):
                    return "math.pow(math.abs({0}), 1f / 3f)";
            }

            var formatParams = Enumerable.Range(0, sig.Params.Length).Select(i => $"{{{i}}}"); // {0}, {1}, ...
            return $"math.{sig.OpType.ToLower()}({string.Join(", ", formatParams)})"; // something like "math.dot({0}, {1}) or math.cos({0})"
        }

        public static int GetVersion()
        {
            return GenerateDelegateCode().Aggregate(0, (h, s) => h ^ s.GetHashCode());
        }

        static string GetOpCodeGen(MathOperationsMetaData.OpSignature sig)
        {
            if (!sig.SupportsMultiInputs())
            {
                var indexedValues = sig.Params.Select((p, index) => $"values[{index}].{p}").ToArray();
                return $"{{ MathGeneratedFunction.{sig.EnumName}, (Value[] values) => {string.Format(GetOpCodeFormat(sig), indexedValues)} }},";
            }

            var aType = sig.Params[0].ToString();
            var bType = sig.Params[1].ToString();
            return $"{{ MathGeneratedFunction.{sig.EnumName}, (Value[] values) =>\n" +
                "\t\t\t{\n" +
                "\t\t\t\tAssert.IsTrue(values.Length >= 2);\n" +
                "\t\t\t\tvar result = values[0];\n" +
                "\t\t\t\tfor (int i = 1; i < values.Length; ++i)\n" +
                $"\t\t\t\t\tresult = {string.Format(GetOpCodeFormat(sig), $"result.{aType}", $"values[i].{bType}")};\n" +
                "\t\t\t\treturn result;\n" +
                "\t\t\t} },";
        }

        static List<string> GenerateDelegateCode()
        {
            return MathOperationsMetaData.SupportedMethods.Select(GetOpCodeGen).ToList();
        }

        [MenuItem("internal:Visual Scripting/Generate Math Functions")]
        static void DumpCode()
        {
            if (EditorUtility.DisplayDialog("Invalidate old graphs", "Warning: generating math functions will invalidate all visual scripts containing previous versions of GenericMathNode. Press OK to proceed anyway.", "OK", "Cancel"))
            {
                var filenameFull = Path.Combine(GetFilePathForCodeGen().FullName, FileName);
                var str = GenerateMathFile();
                WriteFile(filenameFull, str.ToString());
                Debug.Log($"wrote CodeGen version {GetVersion()} to {filenameFull}");
            }
        }

        static StringBuilder GenerateMathFile()
        {
            StringBuilder str = new StringBuilder();

            foreach (var line in new[] { "System", "Unity.Mathematics", "UnityEngine", "Assert = Unity.Assertions.Assert" })
            {
                str.Append("using " + line + ";\n");
            }

            str.Append("\n");
            str.Append("namespace Runtime.Mathematics\n");
            str.Append("{\n");
            str.Append("\tpublic enum " + EnumName + " : ulong\n");
            str.Append("\t{\n");

            for (var index = 0; index < MathOperationsMetaData.SupportedMethods.Count; index++)
            {
                var method = MathOperationsMetaData.SupportedMethods[index];
                var line = $"{method.EnumName} = 0x{method.StableId:X16}UL,";
                str.Append("\t\t" + line + "\n");
            }

            str.Append($"\t\t{LastEnumName} = 0xFFFFFFFF_FFFFFFFFUL,\n");

            str.Append("\t}\n");
            str.Append("\n");
            str.Append("\tpublic static class " + ClassName + "\n");
            str.Append("\t{\n");
            str.Append("\t\tinternal static int GenerationVersion => " + GetVersion() + ";\n");
            str.Append("\n");
            str.Append("\t\tinternal static System.Collections.Generic.Dictionary<MathGeneratedFunction, MathValueDelegate> s_Delegates =\n");
            str.Append("\t\tnew System.Collections.Generic.Dictionary<MathGeneratedFunction, MathValueDelegate>()\n");
            str.Append("\t\t{\n");
            foreach (var delegateCode in GenerateDelegateCode())
            {
                str.Append("\t\t\t" + delegateCode + "\n");
            }
            str.Append("\t\t};\n");
            str.Append("\t}\n");
            str.Append("}\n");

            return str;
        }

        private static void OpFromStableUlong(ulong value, out bool isCustomOp,
            out MathOperationsMetaData.CustomOps customOps, out MathOperationsMetaData.MathOps mathOps, out uint signature)
        {
            uint opPart = (uint)(value & 0x00000000_FFFFFFFF);
            signature = (uint)((value & 0xFFFFFFFF_00000000) >> 32);
            isCustomOp = (opPart & k_IsCustomOpBit) == k_IsCustomOpBit;

            var op = opPart & ~k_IsCustomOpBit;
            if (isCustomOp)
            {
                customOps = (MathOperationsMetaData.CustomOps)op;
                mathOps = MathOperationsMetaData.MathOps.None;
            }
            else
            {
                customOps = MathOperationsMetaData.CustomOps.None;
                mathOps = (MathOperationsMetaData.MathOps)op;
            }
        }

        static uint GenerateSignatureFlag(Runtime.ValueType returnType, Runtime.ValueType[] paramTypes)
        {
            uint sig = 0u;
            sig |= (byte)returnType;
            UnityEngine.Assertions.Assert.IsTrue(paramTypes.Length <= 7); // (7 params + 1 return type) * 4 bits = 32bits
            for (int i = 0; i < paramTypes.Length; i++)
            {
                sig |= (uint)((byte)paramTypes[i]) << (4 * (i + 1));
            }
            return sig;
        }

        internal static ulong GenerateStableValueForOp(MathOperationsMetaData.OpSignature method)
        {
            if (method.IsCustomOp)
                return GenerateStableValueForCustomOp(method.CustomOp, method.Return, method.Params);
            return GenerateStableValueForMathOp(method.MathOp, method.Return, method.Params);
        }

        internal static ulong GenerateStableValueForCustomOp(MathOperationsMetaData.CustomOps customOp,
            Runtime.ValueType returnType, Runtime.ValueType[] paramTypes)
        {
            uint signature = GenerateSignatureFlag(returnType, paramTypes);

            uint op = (uint)customOp;
            Assert.AreNotEqual((op & k_IsCustomOpBit), k_IsCustomOpBit, $"Op {customOp} uses the {nameof(k_IsCustomOpBit)} bit");

            op |= k_IsCustomOpBit;
            ulong result = ((ulong)signature << 32) | op;
            return result;
        }

        public static ulong GenerateStableValueForMathOp(MathOperationsMetaData.MathOps mathOp, ValueType returnType, ValueType[] paramTypes)
        {
            uint signature = GenerateSignatureFlag(returnType, paramTypes);
            uint op = (uint)mathOp;
            Assert.AreNotEqual((op & k_IsCustomOpBit), k_IsCustomOpBit, $"Op {mathOp} uses the {nameof(k_IsCustomOpBit)} bit");
            ulong result = ((ulong)signature << 32) | op;
            return result;
        }

        static DirectoryInfo GetFilePathForCodeGen()
        {
            var assetDirPath = Path.GetDirectoryName(Application.dataPath);
            if (assetDirPath == null)
                throw new InvalidOperationException($"Can't get data path : {Application.dataPath}");
            var projectDir = new DirectoryInfo(assetDirPath).Parent;
            if (projectDir == null)
                throw new InvalidOperationException($"Can't get project path : {Application.dataPath}/..");
            string[] fileDirNames = { projectDir.FullName, "Packages", "com.unity.visualscripting.entities", "Runtime", "Interpreter", "Nodes", "Data", "Math" };
            var fileDirName = Path.Combine(fileDirNames);
            var fileDirPath = new DirectoryInfo(fileDirName);
            if (!fileDirPath.Exists)
                throw new InvalidOperationException($"Can't find path to output file: {fileDirPath.FullName}");
            return fileDirPath;
        }

        static void WriteFile(string filename, string text)
        {
            // Convert all tabs to spaces
            text = text.Replace("\t", "    ");
            // Normalize line endings, convert all EOL to platform EOL (and let git handle it)
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\n", Environment.NewLine);

            // Generate auto generated comment
            text = s_AutoGenHeader + text;

            // Trim trailing spaces that could have come from code gen.
            char[] trim = { ' ' };
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; ++i)
            {
                lines[i] = lines[i].TrimEnd(trim);
            }

            text = string.Join(Environment.NewLine, lines);

            File.WriteAllText(filename, text);
        }

        static string s_AutoGenHeader =
            "//------------------------------------------------------------------------------\n" +
            "// <auto-generated>\n" +
            "//     This code was generated by a tool.\n" +
            "//\n" +
            "//     Changes to this file may cause incorrect behavior and will be lost if\n" +
            "//     the code is regenerated.\n" +
            "// </auto-generated>\n" +
            "//------------------------------------------------------------------------------\n";
    }
}
