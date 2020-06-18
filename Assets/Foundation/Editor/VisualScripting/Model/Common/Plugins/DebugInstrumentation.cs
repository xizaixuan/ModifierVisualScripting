using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Translators;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityEditor.Modifier.VisualScripting.Plugins
{
    public static class InstrumentForInEditorDebugging
    {
        public static InvocationExpressionSyntax RecordValue(IdentifierNameSyntax recorderVariableSyntax, ExpressionSyntax expression, Type returnType, NodeModel node)
        {
            SimpleNameSyntax name = IdentifierName("Record");
            ((SerializableGUID)node.Guid).ToParts(out var guid1, out var guid2);
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    recorderVariableSyntax,
                    name))
                    .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        new[]
                        {
                            Argument(expression),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(guid1))),
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(guid2)))
                        })))
                    // this is critical as it's used to count the number of recorded values when inserting a SetLastCallFrame
                    // leaving the right number of empty values
                    .WithAdditionalAnnotations(new SyntaxAnnotation(Annotations.RecordValueKind));
        }

        public static ExpressionStatementSyntax BuildLastCallFrameExpression(int recordedValuesCount, GUID id, IdentifierNameSyntax recorderVariableSyntax, ExpressionSyntax progressReportingVariableName = null)
        {
            ((SerializableGUID)id).ToParts(out var guid1, out var guid2);

            bool reportProgress = progressReportingVariableName != null;
            var argumentSyntaxes = new ArgumentSyntax[reportProgress ? 4 : 3];

            argumentSyntaxes[0] = Argument(
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(guid1)));
            argumentSyntaxes[1] = Argument(
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(guid2)));
            argumentSyntaxes[2] = Argument(
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(recordedValuesCount)));

            if (reportProgress)
                argumentSyntaxes[3] = Argument(InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        progressReportingVariableName,
                        IdentifierName("GetProgress"))));

            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        recorderVariableSyntax,
                        IdentifierName("SetLastCallFrame")))
                    .WithArgumentList(
                    ArgumentList(
                        SeparatedList(
                            argumentSyntaxes))));
        }
    }
}