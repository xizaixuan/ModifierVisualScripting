using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Modifier.VisualScripting.Model.Translators;
using UnityEngine;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [SearcherItem(typeof(ClassStencil), SearcherContext.Stack, "Debug/" + NodeTitle)]
    [Serializable]
    public class LogNodeModel : HighLevelNodeModel, IHasMainInputPort
    {
        public const string NodeTitle = "Log";

        public enum LogTypes { Message, Warning, Error }

        public LogTypes LogType = LogTypes.Message;

        public IPortModel InputPort { get; private set; }

        protected override void OnDefineNode()
        {
            InputPort = AddDataInputPort<object>("Object");
        }
    }

    [GraphtoolsExtensionMethods]
    public static class LogTranslator
    {
        public static IEnumerable<SyntaxNode> Build(this RoslynTranslator translator, LogNodeModel model,
            IPortModel portModel)
        {
            var obj = translator.BuildPort(model.InputPort).SingleOrDefault() as ExpressionSyntax;
            string methodName;

            switch (model.LogType)
            {
                case LogNodeModel.LogTypes.Message:
                    methodName = nameof(Debug.Log);
                    break;
                case LogNodeModel.LogTypes.Warning:
                    methodName = nameof(Debug.LogWarning);
                    break;
                case LogNodeModel.LogTypes.Error:
                    methodName = nameof(Debug.LogError);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var arg = obj != null ? Argument(obj) : Argument(LiteralExpression(SyntaxKind.NullLiteralExpression));
            yield return RoslynBuilder.MethodInvocation(methodName, IdentifierName(nameof(Debug)), arg);
        }
    }
}