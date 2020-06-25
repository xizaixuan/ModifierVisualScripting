using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Translators;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [Serializable]
    public abstract class BaseInputNodeModel : HighLevelNodeModel
    {
        public enum EventMode { Held, Pressed, Released }

        [PublicAPI]
        public EventMode Mode;
        protected IPortModel InputPort { get; set; }
        protected IPortModel ButtonOutputPort { get; set; }
        protected abstract string MethodName(IPortModel portModel);

        public ExpressionSyntax BuildCall(RoslynTranslator translator, IPortModel portModel, out ExpressionSyntax inputName, out string methodName)
        {
            if (InputPort.IsConnected || InputPort.EmbeddedValue != null)
                inputName = translator.BuildPort(InputPort).FirstOrDefault() as ExpressionSyntax;
            else
                inputName = SyntaxFactory.LiteralExpression(
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxFactory.Token(SyntaxKind.DefaultKeyword));

            var method = RoslynBuilder.MethodInvocation(methodName = MethodName(portModel), typeof(Input).ToTypeSyntax(), SyntaxFactory.Argument(inputName));
            return method;
        }
    }

    [GraphElementsExtensionMethodsCache]
    public static class BaseInputNodeTranslator
    {
        public static IEnumerable<SyntaxNode> BuildGetInput(this RoslynTranslator translator, BaseInputNodeModel model, IPortModel portModel)
        {
            var method = model.BuildCall(translator, portModel, out _, out _);

            yield return method;
        }
    }
}