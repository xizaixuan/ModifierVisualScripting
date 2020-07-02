using System;
using Unity.Modifier.GraphElements;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [GraphElementsExtensionMethodsCache]
    public static class GraphElementFactoryExtensions
    {
        public static IGraphElement CreateNode(this ElementBuilder elementBuilder, IStore store, NodeModel model)
        {
            var ui = new Node();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreatePort(this ElementBuilder elementBuilder, IStore store, PortModel model)
        {
            var ui = new Port();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateEdge(this ElementBuilder elementBuilder, IStore store, EdgeModel model)
        {
            var ui = new Edge();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateStickyNote(this ElementBuilder elementBuilder, IStore store, StickyNoteModel model)
        {
            var ui = new StickyNote();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateGetComponent(this ElementBuilder elementBuilder, IStore store, HighLevelNodeModel model)
        {
            var ui = new CollapsiblePortNode();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateStack(this ElementBuilder elementBuilder, IStore store, StackBaseModel model)
        {
            var ui = new StackNode();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreatePlacemat(this ElementBuilder elementBuilder, IStore store, PlacematModel model)
        {
            var ui = new Placemat();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateBinaryOperator(this ElementBuilder elementBuilder, IStore store, BinaryOperatorNodeModel model)
        {
            var ui = new Node();
            ui.Setup(model, store, elementBuilder.GraphView);
            ui.CustomSearcherHandler = (node, nStore, pos, _) =>
            {
                SearcherService.ShowEnumValues("Pick a new operator type", typeof(BinaryOperatorKind), pos, (pickedEnum, __) =>
                {
                    if (pickedEnum != null)
                    {
                        ((BinaryOperatorNodeModel)node.NodeModel).Kind = (BinaryOperatorKind)pickedEnum;
                        nStore.Dispatch(new RefreshUIAction(UpdateFlags.GraphTopology));
                    }
                });
                return true;
            };
            return ui;
        }

        public static IGraphElement CreateUnaryOperator(this ElementBuilder elementBuilder, IStore store, UnaryOperatorNodeModel model)
        {
            var ui = new Node();
            ui.Setup(model, store, elementBuilder.GraphView);
            ui.CustomSearcherHandler = (node, nStore, pos, _) =>
            {
                SearcherService.ShowEnumValues("Pick a new operator type", typeof(UnaryOperatorKind), pos, (pickedEnum, __) =>
                {
                    if (pickedEnum != null)
                    {
                        ((UnaryOperatorNodeModel)node.NodeModel).Kind = (UnaryOperatorKind)pickedEnum;
                        nStore.Dispatch(new RefreshUIAction(UpdateFlags.GraphTopology));
                    }
                });
                return true;
            };
            return ui;
        }

        public static IGraphElement CreateToken(this ElementBuilder elementBuilder, IStore store, IVariableModel model)
        {
            var isExposed = model.DeclarationModel?.IsExposed;
            Texture2D icon = (isExposed != null && isExposed.Value)
                ? GraphViewStaticBridge.LoadIconRequired("GraphView/Nodes/BlackboardFieldExposed.png")
                : null;

            var ui = new Token();
            ui.Setup(model, store, elementBuilder.GraphView, icon);
            return ui;
        }

        public static IGraphElement CreateConstantToken(this ElementBuilder elementBuilder, IStore store, IConstantNodeModel model)
        {
            var ui = new Token();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateToken(this ElementBuilder elementBuilder, IStore store, IStringWrapperConstantModel model)
        {
            return CreateConstantToken(elementBuilder, store, model);
        }

        public static IGraphElement CreateToken(this ElementBuilder elementBuilder, IStore store, SystemConstantNodeModel model)
        {
            var ui = new Token();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateEdgePortal(this ElementBuilder elementBuilder, IStore store, IEdgePortalModel model)
        {
            var ui = new Token();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }
    }
}
