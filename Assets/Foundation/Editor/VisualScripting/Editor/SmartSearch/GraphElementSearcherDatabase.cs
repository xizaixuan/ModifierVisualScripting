using System.Reflection;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Modifier.EditorCommon.Extensions;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Modifier.VisualScripting;
using UnityEditor.Modifier.EditorCommon.Utility;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    [Flags]
    public enum MemberFlags
    {
        Constructor,
        Extension,
        Field,
        Method,
        Property
    }

    [PublicAPI]
    public class GraphElementSearcherDatabase
    {
        public enum IfConditionMode
        {
            Basic,
            Advanced,
            Complete
        }

        const string k_Constant = "Constant";
        const string k_ControlFlow = "Control Flow";
        const string k_LoopStack = "... Loop Stack";
        const string k_Operator = "Operator";
        const string k_InlineExpression = "Inline Expression";
        const string k_InlineLabel = "10+y";
        const string k_Stack = "Stack";
        const string k_NewFunction = "Create New Function";
        const string k_FunctionName = "My Function";
        const string k_Sticky = "Sticky Note";
        const string k_Then = "then";
        const string k_Else = "else";
        const string k_IfCondition = "If Condition";
        const string k_FunctionMembers = "Function Members";
        const string k_GraphVariables = "Graph Variables";
        const string k_Function = "Function";
        const string k_Graphs = "Graphs";
        const string k_Macros = "Macros";
        const string k_Macro = "Macro";

        static readonly Vector2 k_ThenStackOffset = new Vector2(-220, 300);
        static readonly Vector2 k_ElseStackOffset = new Vector2(170, 300);
        static readonly Vector2 k_ClosedFlowStackOffset = new Vector2(-25, 450);

        // TODO: our builder methods ("AddStack",...) all use this field. Users should be able to create similar methods. making it public until we find a better solution
        public readonly List<SearcherItem> Items;
        public readonly Stencil Stencil;

        public GraphElementSearcherDatabase(Stencil stencil)
        {
            Stencil = stencil;
            Items = new List<SearcherItem>();
        }

        public GraphElementSearcherDatabase AddNodesWithSearcherItemAttribute()
        {
            var types = TypeCache.GetTypesWithAttribute<SearcherItemAttribute>();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes<SearcherItemAttribute>().ToList();
                if (!attributes.Any())
                    continue;

                foreach (var attribute in attributes)
                {
                    if (!attribute.StencilType.IsInstanceOfType(Stencil))
                        continue;

                    var name = attribute.Path.Split('/').Last();
                    var path = attribute.Path.Remove(attribute.Path.LastIndexOf('/') + 1);

                    switch (attribute.Context)
                    {
                        case SearcherContext.Graph:
                            {
                                var node = new GraphNodeModelSearcherItem(
                                    new NodeSearcherItemData(type),
                                    data => data.CreateNode(type, name),
                                    name
                                );
                                Items.AddAtPath(node, path);
                                break;
                            }

                        case SearcherContext.Stack:
                            {
                                var node = new StackNodeModelSearcherItem(
                                    new NodeSearcherItemData(type),
                                    data => data.CreateNode(type, name),
                                    name
                                );
                                Items.AddAtPath(node, path);
                                break;
                            }

                        default:
                            Debug.LogWarning($"The node {type} is not a {SearcherContext.Stack} or " +
                                $"{SearcherContext.Graph} node, so it cannot be add in the Searcher");
                            break;
                    }

                    break;
                }
            }

            return this;
        }

        public GraphElementSearcherDatabase AddStickyNote()
        {
            var node = new GraphNodeModelSearcherItem(
                new SearcherItemData(SearcherItemTarget.StickyNote),
                data =>
                {
                    var rect = new Rect(data.Position, StickyNote.defaultSize);
                    var vsGraphModel = (VSGraphModel)data.GraphModel;
                    return vsGraphModel.CreateStickyNote(rect, data.SpawnFlags);
                },
                k_Sticky
            );
            Items.AddAtPath(node);

            return this;
        }

        public GraphElementSearcherDatabase AddStack()
        {
            var node = new GraphNodeModelSearcherItem(
                new SearcherItemData(SearcherItemTarget.Stack),
                data => data.CreateStack(string.Empty),
                k_Stack
            );
            Items.AddAtPath(node);

            return this;
        }

        public GraphElementSearcherDatabase AddBinaryOperators()
        {
            SearcherItem parent = SearcherItemUtility.GetItemFromPath(Items, k_Operator);

            foreach (BinaryOperatorKind kind in Enum.GetValues(typeof(BinaryOperatorKind)))
            {
                parent.AddChild(new GraphNodeModelSearcherItem(
                    new BinaryOperatorSearcherItemData(kind),
                    data => data.CreateBinaryOperatorNode(kind),
                    kind.ToString()
                ));
            }

            return this;
        }

        public GraphElementSearcherDatabase AddUnaryOperators()
        {
            SearcherItem parent = SearcherItemUtility.GetItemFromPath(Items, k_Operator);

            foreach (UnaryOperatorKind kind in Enum.GetValues(typeof(UnaryOperatorKind)))
            {
                if (kind == UnaryOperatorKind.PostDecrement || kind == UnaryOperatorKind.PostIncrement)
                {
                    parent.AddChild(new StackNodeModelSearcherItem(
                        new UnaryOperatorSearcherItemData(kind),
                        data => data.CreateUnaryStatementNode(kind),
                        kind.ToString()
                    ));
                    continue;
                }

                parent.AddChild(new GraphNodeModelSearcherItem(
                    new UnaryOperatorSearcherItemData(kind),
                    data => data.CreateUnaryStatementNode(kind),
                    kind.ToString()
                ));
            }

            return this;
        }

        public GraphElementSearcherDatabase AddConstants(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                AddConstants(type);
            }

            return this;
        }

        public GraphElementSearcherDatabase AddConstants(Type type)
        {
            TypeHandle handle = type.GenerateTypeHandle(Stencil);

            SearcherItem parent = SearcherItemUtility.GetItemFromPath(Items, k_Constant);
            parent.AddChild(new GraphNodeModelSearcherItem(
                new TypeSearcherItemData(handle, SearcherItemTarget.Constant),
                data => data.CreateConstantNode("", handle),
                $"{type.FriendlyName().Nicify()} {k_Constant}"
            ));

            return this;
        }

        public GraphElementSearcherDatabase AddGraphVariables(IGraphModel graphModel)
        {
            SearcherItem parent = null;
            var vsGraphModel = (VSGraphModel)graphModel;

            foreach (IVariableDeclarationModel declarationModel in vsGraphModel.GraphVariableModels)
            {
                if (parent == null)
                {
                    parent = SearcherItemUtility.GetItemFromPath(Items, k_GraphVariables);
                }

                parent.AddChild(new GraphNodeModelSearcherItem(
                    new TypeSearcherItemData(declarationModel.DataType, SearcherItemTarget.Variable),
                    data => data.CreateVariableNode(declarationModel),
                    declarationModel.Name.Nicify()
                ));
            }

            return this;
        }

        public SearcherDatabase Build()
        {
            return SearcherDatabase.Create(Items, "", false);
        }
    }
}