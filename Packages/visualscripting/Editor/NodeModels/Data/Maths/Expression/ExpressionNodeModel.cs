using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Modifier.NodeModels;
using Modifier.Runtime;
using Modifier.Runtime.Mathematics;
using Unity.Assertions;
using Unity.Modifier.GraphElements;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.EditorCommon.Extensions;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using Object = UnityEngine.Object;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil.Expression
{
    [Serializable, DotsSearcherItem("Math/" + k_Title)]
    class ExpressionNodeModel : BaseDotsNodeModel, IHasMainOutputPort, IRenamable
    {
        private const string k_Title = "Expression";
        public override string Title => Expression;
        public IPortModel OutputPort { get; private set; }

        [SerializeField] string m_Expression = "10 + 2";
        [SerializeField] ValueType[] m_VariableTypes;

        public string Expression
        {
            get => m_Expression;
            set => m_Expression = value;
        }

        public override void OnConnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
            base.OnConnection(selfConnectedPortModel, otherConnectedPortModel);
            if (selfConnectedPortModel.Direction == Direction.Input)
            {
                var i = InputsByDisplayOrder.IndexOf(selfConnectedPortModel);
                if (m_VariableTypes == null)
                    m_VariableTypes = new ValueType[i + 1];
                else if (m_VariableTypes.Length < i + 1)
                    Array.Resize(ref m_VariableTypes, i + 1);
                m_VariableTypes[i] =
                    otherConnectedPortModel?.DataTypeHandle.ToValueTypeOrUnknown() ?? ValueType.Unknown;
            }

            DefineNode();
        }

        public override void OnDisconnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
            base.OnDisconnection(selfConnectedPortModel, otherConnectedPortModel);
            OnConnection(selfConnectedPortModel, otherConnectedPortModel);
        }

        protected override void OnDefineNode()
        {
            var root = Parser.Parse(m_Expression, out string error);

            ValueType outputType = ValueType.Float;
            if (error == null)
            {
                Dictionary<string, (int, ValueType)> variables = new Dictionary<string, (int, ValueType)>();
                outputType = GetAllVariables(variables, root);
                foreach (var variable in variables)
                    AddDataInputPort(variable.Key, variable.Value.Item2.ValueTypeToTypeHandle());
                m_VariableTypes = variables.OrderBy(x => x.Value.Item1).Select(x => x.Value.Item2).ToArray();
            }
            else
                Debug.LogError("Error during Expression Node parsing: " + error, VSGraphModel.AssetModel as Object);
            OutputPort = AddDataOutputPort(null, outputType.ValueTypeToTypeHandle(), nameof(OutputPort));

            ValueType GetAllVariables(Dictionary<string, (int, ValueType)> variables, INode n)
            {
                switch (n)
                {
                    case Variable v:
                        if (variables.TryGetValue(v.Id, out var t))
                            return t.Item2;

                        t = (variables.Count, ValueType.Float);

                        // OnConnection changes the type of the portmodel, which is then moved to PreviousInputs by DefineNode
                        // if (m_PreviousInputs.TryGetValue(v.Id, out var portModel))
                        // t.Item2 = portModel.DataType.TypeHandleToValueTypeOrUnknown();
                        // first OnEnable/OnDefine won't have previous types, rely on serialization
                        if (m_VariableTypes != null && t.Item1 < m_VariableTypes.Length)
                            t.Item2 = m_VariableTypes[t.Item1];
                        variables.Add(v.Id, t);

                        return t.Item2;
                    case UnOp u:
                        return GetAllVariables(variables, u.A);
                    case BinOp b:
                        var tA = GetAllVariables(variables, b.A);
                        var tB = GetAllVariables(variables, b.B);
                        var (_, signature) = AnalyzeBinOp(b, tA, tB);
                        return signature.Return;
                    case FuncCall f:
                        if (!MathOperationsMetaData.GetMethodForOpAndArgumentTypes(f.Id, out _, out var sig, f.Arguments.Select(a => GetAllVariables(variables, a))))
                            throw new InvalidDataException($"Cannot find a matching overload of operation '{f.Id}'");
                        return sig.Return;
                    case ExpressionValue _:
                        return ValueType.Float;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public bool IsRenamable => true;

        public void Rename(string newName)
        {
            Expression = newName;
            ((VSGraphModel)GraphModel).LastChanges.ChangedElements.Add(this);
            DefineNode();
        }

        public bool Translate(GraphBuilder builder, out Runtime.INode node, out PortMapper portToOffsetMapping,
            out uint? preAllocatedDataIndex)
        {
            preAllocatedDataIndex = null;
            portToOffsetMapping = new PortMapper();
            node = default;

            var root = Parser.Parse(m_Expression, out string error);
            if (error != null)
                return false;
            var builderLastPortIndex = builder.LastPortIndex;
            if (root is Variable) // id function
                throw new NotImplementedException("id function");

            var mapper = portToOffsetMapping;
            Dictionary<string, (Passthrough passthrough, ValueType variableType)> variables = new Dictionary<string, (Passthrough passthrough, ValueType variableType)>();

            var outputPort = Visit(root);
            portToOffsetMapping.Add(OutputPort.UniqueId, Direction.Output, outputPort.Item1.Port.Index - builderLastPortIndex);
            builder.ReplaceNodeModelMapping(this, portToOffsetMapping, null, builderLastPortIndex);


            return false;

            MathGenericNode MakeMathNode(MathGeneratedFunction mathGeneratedFunction, int inputCount)
            {
                var mathGenericNode = new MathGenericNode
                { GenerationVersion = MathGeneratedDelegates.GenerationVersion };
                mathGenericNode.Function = mathGeneratedFunction;
                mathGenericNode.Inputs.DataCount = inputCount;
                return mathGenericNode;
            }

            (OutputDataPort, ValueType) Visit(INode n)
            {
                switch (n)
                {
                    case ExpressionValue val:
                        return (builder.AddNode(new ConstantFloat { Value = val.F }).ValuePort, ValueType.Float);
                    case Variable v:
                        (Passthrough passthrough, ValueType variableType) variableData;
                        if (!variables.TryGetValue(v.Id, out variableData))
                        {
                            variableData.passthrough = builder.AddNode(new Passthrough());
                            mapper.Add(v.Id, Direction.Input, variableData.passthrough.Input.Port.Index - builderLastPortIndex);
                            var portModel = InputsById[v.Id];
                            variableData.variableType = portModel.IsConnected
                                ? portModel.ConnectionPortModels.First().DataTypeHandle.ToValueTypeOrUnknown()
                                : ValueType.Float;
                            variables.Add(v.Id, (variableData.passthrough, variableData.variableType));
                        }

                        return (variableData.passthrough.Output, variableData.variableType);
                    case FuncCall f:
                        {
                            var arguments = f.Arguments.Select(Visit).ToArray();

                            if (!MathOperationsMetaData.GetMethodForOpAndArgumentTypes(f.Id, out var mathGeneratedFunction, out var sig, arguments.Select(x => x.Item2)))
                                throw new InvalidDataException($"Cannot find a matching overload of operation '{f.Id}'");

                            var mathGenericNode = MakeMathNode(mathGeneratedFunction, sig.Params.Length);
                            mathGenericNode = builder.AddNode(mathGenericNode);

                            if (f.Arguments.Count != sig.Params.Length)
                                throw new InvalidDataException($"function \"{sig.OpType}\" expects {sig.Params.Length} arguments, received {f.Arguments.Count}\nFunction signature: {sig}");
                            for (int i = 0; i < sig.Params.Length; i++)
                                builder.CreateEdge(arguments[i].Item1, mathGenericNode.Inputs.SelectPort((uint)i));

                            return (mathGenericNode.Result, sig.Return);
                        }
                    case UnOp un:
                        {
                            Assert.AreEqual(un.Type, OpType.Minus);
                            var negate = builder.AddNode(new Negate());
                            var negatedValue = Visit(un.A);
                            builder.CreateEdge(negatedValue.Item1, negate.Input);
                            return (negate.Output, negatedValue.Item2);
                        }
                    case BinOp bin:
                        {
                            var opandA = Visit(bin.A);
                            var opandB = Visit(bin.B);

                            var (mathGeneratedFunction, signature) = AnalyzeBinOp(bin, opandA.Item2, opandB.Item2);
                            var mathGenericNode = MakeMathNode(mathGeneratedFunction, 2);
                            mathGenericNode = builder.AddNode(mathGenericNode);
                            builder.CreateEdge(opandA.Item1, mathGenericNode.Inputs.SelectPort(0));
                            builder.CreateEdge(opandB.Item1, mathGenericNode.Inputs.SelectPort(1));

                            return (mathGenericNode.Result, signature.Return);
                        }
                    default: throw new NotImplementedException();
                }
            }
        }

        private static (MathGeneratedFunction mathGeneratedFunction, MathOperationsMetaData.OpSignature signature) AnalyzeBinOp(BinOp bin,
            ValueType opandA, ValueType opandB)
        {
            MathOperationsMetaData.CustomOps customOps;
            switch (bin.Type)
            {
                case OpType.Add:
                    customOps = MathOperationsMetaData.CustomOps.Add;
                    break;
                case OpType.Sub:
                    customOps = MathOperationsMetaData.CustomOps.Subtract;
                    break;
                case OpType.Mul:
                    customOps = MathOperationsMetaData.CustomOps.Multiply;
                    break;
                case OpType.Div:
                    customOps = MathOperationsMetaData.CustomOps.Divide;
                    break;
                case OpType.Mod:
                    customOps = MathOperationsMetaData.CustomOps.Modulo;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            if (!MathOperationsMetaData.GetMethodForOpAndArgumentTypes(customOps.ToString(), out var mathGeneratedFunction,
                out var signature, new[] { opandA, opandB }))
                throw new InvalidDataException($"Cannot find a matching overload of operation '{customOps}'");
            return (mathGeneratedFunction, signature);
        }
    }

    enum OpType
    {
        Add,
        Sub,
        Mul,
        Div,
        LeftParens,
        RightParens,
        Plus,
        Minus,
        Coma,
        Mod
    }

    interface INode
    {
    }

    interface IOp : INode
    {
    }

    interface IVal : INode
    {
    }

    struct UnOp : IOp
    {
        public readonly OpType Type;
        public readonly INode A;

        public UnOp(OpType type, INode a)
        {
            Type = type;
            A = a;
        }

        public override string ToString() => $"{Parser.Ops[Type].Str}{A}";
    }

    struct BinOp : IOp
    {
        public readonly OpType Type;
        public readonly INode A;
        public readonly INode B;

        public BinOp(OpType type, INode a, INode b)
        {
            Type = type;
            A = a;
            B = b;
        }

        public override string ToString() => $"({A} {Parser.Ops[Type].Str} {B})";
    }

    struct FuncCall : IOp
    {
        public readonly string Id;
        public readonly List<INode> Arguments;

        public FuncCall(string id, List<INode> arguments)
        {
            Id = id;
            Arguments = arguments;
        }

        public override string ToString() => $"#{Id}({string.Join(", ", Arguments)})";
    }

    struct ExpressionValue : IVal
    {
        public readonly float F;

        public ExpressionValue(float f)
        {
            F = f;
        }

        public override string ToString() => F.ToString(CultureInfo.InvariantCulture);
    }

    struct Variable : IVal
    {
        public readonly string Id;

        public Variable(string id)
        {
            Id = id;
        }

        public override string ToString() => $"${Id}";
    }

    [Flags]
    public enum Token
    {
        None = 0,
        Op = 1,
        Number = 2,
        Identifier = 4,
        LeftParens = 8,
        RightParens = 16,
        Coma = 32,
    }

    static class Parser
    {
        internal struct Operator
        {
            public readonly OpType Type;
            public readonly string Str;
            public readonly int Precedence;
            public readonly Associativity Associativity;
            public readonly bool Unary;

            public Operator(OpType type, string str, int precedence, Associativity associativity = Associativity.None,
                            bool unary = false)
            {
                Type = type;
                Str = str;
                Precedence = precedence;
                Associativity = associativity;
                Unary = unary;
            }
        }

        internal enum Associativity
        {
            None,
            Left,
            Right,
        }

        internal static readonly Dictionary<OpType, Operator> Ops = new Dictionary<OpType, Operator>
        {
            {OpType.Add, new Operator(OpType.Add, "+", 2, Associativity.Left)},
            {OpType.Sub, new Operator(OpType.Sub, "-", 2, Associativity.Left)},

            {OpType.Mul, new Operator(OpType.Mul, "*", 3, Associativity.Left)},
            {OpType.Div, new Operator(OpType.Div, "/", 3, Associativity.Left)},
            {OpType.Mod, new Operator(OpType.Mod, "%", 3, Associativity.Left)},

            {OpType.LeftParens, new Operator(OpType.LeftParens, "(", 5)},

            // {OpType.Coma, new Operator(OpType.Coma, ",", 1000, Associativity.Left)},

            // {OpType.Plus, new Operator(OpType.Plus, "+", 2000, Associativity.Right, unary: true)},
            {OpType.Minus, new Operator(OpType.Minus, "-", 2000, Associativity.Right, unary: true)},
        };

        static Operator ReadOperator(string input, bool unary)
        {
            return Ops.Single(o => o.Value.Str == input && o.Value.Unary == unary).Value;
        }

        public static INode Parse(string s, out string error)
        {
            var output = new Stack<INode>();
            var opStack = new Stack<Operator>();

            Reader r = new Reader(s);

            try
            {
                r.ReadToken();
                error = null;
                return ParseUntil(r, opStack, output, Token.None, 0);
            }
            catch (Exception e)
            {
                error = e.Message;
                return null;
            }
        }

        public static bool TryPeek<T>(this Stack<T> stack, out T t)
        {
            if (stack.Count != 0)
            {
                t = stack.Peek();
                return true;
            }

            t = default;
            return false;
        }

        private static INode ParseUntil(Reader r, Stack<Operator> opStack, Stack<INode> output, Token readUntilToken,
            int startOpStackSize)
        {
            do
            {
                switch (r.CurrentTokenType)
                {
                    case Token.LeftParens:
                        {
                            opStack.Push(Ops[OpType.LeftParens]);
                            r.ReadToken();
                            INode arg = ParseUntil(r, opStack, output, Token.Coma | Token.RightParens,
                                opStack.Count);
                            if (r.CurrentTokenType == Token.Coma)
                                throw new InvalidDataException("Tuples not supported");
                            if (r.CurrentTokenType != Token.RightParens)
                                throw new InvalidDataException("Mismatched parens, missing a closing parens");
                            output.Push(arg);

                            while (opStack.TryPeek(out var stackOp) && stackOp.Type != OpType.LeftParens)
                            {
                                opStack.Pop();
                                PopOpOpandsAndPushNode(stackOp);
                            }

                            if (opStack.TryPeek(out var leftParens) && leftParens.Type == OpType.LeftParens)
                                opStack.Pop();
                            else
                                throw new InvalidDataException("Mismatched parens");
                            r.ReadToken();
                            break;
                        }
                    case Token.RightParens:
                        throw new InvalidDataException("Mismatched parens");
                    case Token.Op:
                        {
                            bool unary = r.PrevTokenType == Token.Op ||
                                r.PrevTokenType == Token.LeftParens ||
                                r.PrevTokenType == Token.None;
                            var readBinOp = ReadOperator(r.CurrentToken, unary);

                            while (opStack.TryPeek(out var stackOp) &&
                                   // the operator at the top of the operator stack is not a left parenthesis or coma
                                   stackOp.Type != OpType.LeftParens && stackOp.Type != OpType.Coma &&
                                   // there is an operator at the top of the operator stack with greater precedence
                                   (stackOp.Precedence > readBinOp.Precedence ||
                                    // or the operator at the top of the operator stack has equal precedence and the token is left associative
                                    stackOp.Precedence == readBinOp.Precedence &&
                                    readBinOp.Associativity == Associativity.Left))
                            {
                                opStack.Pop();
                                PopOpOpandsAndPushNode(stackOp);
                            }

                            opStack.Push(readBinOp);
                            r.ReadToken();
                            break;
                        }
                    case Token.Number:
                        output.Push(new ExpressionValue(float.Parse(r.CurrentToken, CultureInfo.InvariantCulture)));
                        r.ReadToken();
                        break;
                    case Token.Identifier:
                        var id = r.CurrentToken;
                        r.ReadToken();
                        if (r.CurrentTokenType != Token.LeftParens) // variable
                        {
                            output.Push(new Variable(id));
                            break;
                        }
                        else // function call
                        {
                            r.ReadToken(); // skip (
                            List<INode> args = new List<INode>();

                            while (true)
                            {
                                INode arg = ParseUntil(r, opStack, output, Token.Coma | Token.RightParens,
                                    opStack.Count);
                                args.Add(arg);
                                if (r.CurrentTokenType == Token.RightParens)
                                {
                                    break;
                                }
                                r.ReadToken();
                            }

                            r.ReadToken(); // skip )

                            // RecurseThroughArguments(args, arg);
                            output.Push(new FuncCall(id, args));
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException(r.CurrentTokenType.ToString());
                }
            }
            while (!readUntilToken.HasFlag(r.CurrentTokenType));

            while (opStack.Count > startOpStackSize)
            {
                var readBinOp = opStack.Pop();
                if (readBinOp.Type == OpType.LeftParens)
                    break;
                PopOpOpandsAndPushNode(readBinOp);
            }

            return output.Pop();

            void PopOpOpandsAndPushNode(Operator readBinOp)
            {
                var b = output.Pop();
                if (readBinOp.Unary)
                {
                    output.Push(new UnOp(readBinOp.Type, b));
                }
                else
                {
                    var a = output.Pop();
                    output.Push(new BinOp(readBinOp.Type, a, b));
                }
            }

            void RecurseThroughArguments(List<INode> args, INode n)
            {
                switch (n)
                {
                    case BinOp b when b.Type == OpType.Coma:
                        RecurseThroughArguments(args, b.A);
                        RecurseThroughArguments(args, b.B);
                        break;
                    default:
                        args.Add(n);
                        break;
                }
            }
        }
    }

    internal class Reader
    {
        private readonly string _input;
        private int _i;

        public Reader(string input)
        {
            _input = input.Trim();
            _i = 0;
        }

        private void SkipWhitespace()
        {
            while (!Done && Char.IsWhiteSpace(_input[_i]))
                _i++;
        }

        public bool Done => _i >= _input.Length;
        private char NextChar => _input[_i];
        private char ConsumeChar() => _input[_i++];

        public string CurrentToken;
        public Token CurrentTokenType;
        public Token PrevTokenType;

        public void ReadToken()
        {
            CurrentToken = null;
            PrevTokenType = CurrentTokenType;
            CurrentTokenType = Token.None;
            if (Done)
                return;
            if (NextChar == '(')
            {
                ConsumeChar();
                CurrentTokenType = Token.LeftParens;
            }
            else if (NextChar == ')')
            {
                ConsumeChar();
                CurrentTokenType = Token.RightParens;
            }
            else if (NextChar == ',')
            {
                ConsumeChar();
                CurrentTokenType = Token.Coma;
            }
            else if (Char.IsDigit(NextChar) || NextCharIsPoint())
            {
                bool foundPoint = false;
                StringBuilder sb = new StringBuilder();
                do
                {
                    foundPoint |= NextCharIsPoint();
                    sb.Append(ConsumeChar());
                }
                while (!Done && (Char.IsDigit(NextChar) || (NextChar == '.' && !foundPoint)));
                if (!Done && foundPoint && NextCharIsPoint()) // 1.2.3
                    throw new InvalidDataException($"Invalid number: '{sb}.'");

                CurrentToken = sb.ToString();
                CurrentTokenType = Token.Number;
            }
            else
            {
                if (MatchOp(out var op))
                {
                    CurrentToken = op.Str;
                    CurrentTokenType = Token.Op;
                    for (int i = 0; i < CurrentToken.Length; i++)
                        ConsumeChar();
                }
                else
                {
                    CurrentTokenType = Token.Identifier;
                    StringBuilder sb = new StringBuilder();
                    while (!Done && NextChar != ')' && NextChar != ',' && !MatchOp(out _) && !Char.IsWhiteSpace(NextChar))
                        sb.Append(ConsumeChar());
                    CurrentToken = sb.ToString();
                }
            }

            SkipWhitespace();

            bool MatchOp(out Parser.Operator desc)
            {
                foreach (var pair in Parser.Ops)
                {
                    if (_input.IndexOf(pair.Value.Str, _i, StringComparison.Ordinal) != _i)
                        continue;
                    desc = pair.Value;
                    return true;
                }

                desc = default;
                return false;
            }

            bool NextCharIsPoint() => NextChar == '.';
        }
    }
}
