using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model.Translators
{
    [GraphElementsExtensionMethodsCache]
    public static class RoslynTranslatorExtensions
    {
        public static IEnumerable<SyntaxNode> BuildThisNode(this RoslynTranslator translator, ThisNodeModel model, IPortModel portModel)
        {
            yield return SyntaxFactory.ThisExpression();
        }

        public static IEnumerable<SyntaxNode> BuildGetPropertyNode(this RoslynTranslator translator, GetPropertyGroupNodeModel model, IPortModel portModel)
        {
            var instancePort = model.InstancePort;
            var input = !instancePort.IsConnected ? SyntaxFactory.ThisExpression() : translator.BuildPort(instancePort).SingleOrDefault();

            if (input == null)
                yield break;

            var member = model.Members.FirstOrDefault(m => m.GetId() == portModel.UniqueId);
            if (member.Path == null || member.Path.Count == 0)
                yield break;

            var access = RoslynBuilder.MemberReference(input, member.Path.ToArray());

            yield return access;
        }

        public static IEnumerable<SyntaxNode> BuildBinaryOperator(this RoslynTranslator translator, BinaryOperatorNodeModel model, IPortModel portModel)
        {
            yield return RoslynBuilder.BinaryOperator(model.Kind,
                translator.BuildPort(model.InputPortA).SingleOrDefault(),
                translator.BuildPort(model.InputPortB).SingleOrDefault());
        }

        public static IEnumerable<SyntaxNode> BuildUnaryOperator(this RoslynTranslator translator, UnaryOperatorNodeModel model, IPortModel portModel)
        {
            var semantic = model.Kind == UnaryOperatorKind.PostDecrement ||
                model.Kind == UnaryOperatorKind.PostIncrement
                ? RoslynTranslator.PortSemantic.Write
                : RoslynTranslator.PortSemantic.Read;
            yield return RoslynBuilder.UnaryOperator(model.Kind, translator.BuildPort(model.InputPort, semantic).SingleOrDefault());
        }

        public static IEnumerable<SyntaxNode> BuildSetVariable(this RoslynTranslator translator, SetVariableNodeModel statement, IPortModel portModel)
        {
            var decl = translator.BuildPort(statement.InstancePort).SingleOrDefault();
            var value = translator.BuildPort(statement.ValuePort).SingleOrDefault();
            yield return decl == null || value == null ? null : RoslynBuilder.Assignment(decl, value);
        }

        public static IEnumerable<SyntaxNode> BuildVariable(this RoslynTranslator translator, IVariableModel v, IPortModel portModel)
        {
            if (v is IConstantNodeModel constantNodeModel)
            {
                if (constantNodeModel.ObjectValue != null)
                {
                    if (constantNodeModel is IStringWrapperConstantModel)
                        yield return translator.Constant(constantNodeModel.ObjectValue.ToString(), translator.Stencil);
                    else
                        yield return translator.Constant(constantNodeModel.ObjectValue, translator.Stencil);
                }

                yield break;
            }

            switch (v.DeclarationModel.VariableType)
            {
                case VariableType.GraphVariable:
                case VariableType.ComponentQueryField:
                    yield return RoslynBuilder.LocalVariableReference(v.DeclarationModel.Name);
                    break;

                //                case VariableType.Literal:
                //                case VariableType.InlineExpression:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static InvocationExpressionSyntax FunctionInvokeExpression(string genericTypeName, string methodName, ExpressionSyntax instance, List<ArgumentSyntax> argumentList)
        {
            var invocationExpressionSyntax = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    instance,
                    SyntaxFactory.IdentifierName("Call")));

            var syntaxNodeList = new SyntaxNodeOrToken[]
            {
                SyntaxFactory.Argument(instance),
                SyntaxFactory.Token(SyntaxKind.CommaToken),
                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(methodName)))
            };

            invocationExpressionSyntax = invocationExpressionSyntax.WithArgumentList(
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(syntaxNodeList)));

            return invocationExpressionSyntax;
        }

        public static IEnumerable<SyntaxNode> BuildSetPropertyNode(this RoslynTranslator translator, SetPropertyGroupNodeModel model, IPortModel portModel)
        {
            SyntaxNode leftHand;

            IPortModel instancePort = model.InstancePort;
            if (!instancePort.IsConnected)
                leftHand = SyntaxFactory.ThisExpression();
            else
                leftHand = translator.BuildPort(instancePort).SingleOrDefault();

            foreach (var member in model.Members)
            {
                string memberId = member.GetId();
                IPortModel inputPort = model.InputsById[memberId];

                SyntaxNode rightHandExpression = translator.BuildPort(inputPort).SingleOrDefault();
                if (rightHandExpression == null)
                    continue;

                var access = RoslynBuilder.MemberReference(leftHand, member.Path.ToArray());

                yield return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, access, rightHandExpression as ExpressionSyntax);
            }
        }

        public static IEnumerable<SyntaxNode> BuildStaticConstantNode(this RoslynTranslator translator, SystemConstantNodeModel model, IPortModel portModel)
        {
            yield return SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName(model.DeclaringType.Name(translator.Stencil)),
                SyntaxFactory.IdentifierName(model.Identifier));
        }
    }
}