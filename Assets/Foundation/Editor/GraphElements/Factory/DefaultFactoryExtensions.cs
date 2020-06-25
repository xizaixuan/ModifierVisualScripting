using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.EditorCommon.Redux;

namespace Unity.Modifier.GraphElements
{
    [GraphElementsExtensionMethodsCache]
    public static class DefaultFactoryExtensions
    {
        public static IGraphElement CreateCollapsiblePortNode(this ElementBuilder elementBuilder, IStore store, IGTFNodeModel model)
        {
            IGraphElement ui;

            if (model is IGTFStackNodeModel)
                ui = new StackNode();
            else if (model is IHasSingleInputPort || model is IHasSingleOutputPort)
                ui = new TokenNode();
            else if (model is IHasPorts)
                ui = new CollapsiblePortNode();
            else
                ui = new Node();

            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreatePort(this ElementBuilder elementBuilder, IStore store, IGTFPortModel model)
        {
            var ui = new Port();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateEdge(this ElementBuilder elementBuilder, IStore store, IGTFEdgeModel model)
        {
            var ui = new Edge();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateStickyNote(this ElementBuilder elementBuilder, IStore store, IGTFStickyNoteModel model)
        {
            var ui = new StickyNote();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreatePlacemat(this ElementBuilder elementBuilder, IStore store, IGTFPlacematModel model)
        {
            var ui = new Placemat();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }
    }
}