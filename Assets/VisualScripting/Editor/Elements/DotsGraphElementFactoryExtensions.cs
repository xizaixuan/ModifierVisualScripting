using Modifier.DotsStencil.Expression;
using Modifier.Elements;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.Editor;
using Node = UnityEditor.Modifier.VisualScripting.Editor.Node;
using Token = UnityEditor.Modifier.VisualScripting.Editor.Token;

namespace Modifier.DotsStencil
{
    [GraphElementsExtensionMethodsCache]
    static class DotsGraphElementFactoryExtensions
    {
        public static IGraphElement CreateInlineExpressionNode(this ElementBuilder elementBuilder, IStore store, ExpressionNodeModel model)
        {
            var ui = new Node();
            ui.AddToClassList(Unity.Modifier.GraphElements.Node.k_UssClassName + "--expression-node");
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateSmartObject(this ElementBuilder elementBuilder, IStore store, SmartObjectReferenceNodeModel model)
        {
            if (model.NeedsUpdate())
                model.DefineNode();

            var ui = new SmartObjectReferenceNode();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateSmartObject(this ElementBuilder elementBuilder, IStore store, SubgraphReferenceNodeModel model)
        {
            if (model.NeedsUpdate())
                model.DefineNode();

            var ui = new SmartObjectReferenceNode();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }

        public static IGraphElement CreateSetVar(this ElementBuilder elementBuilder, IStore store, SetVariableNodeModel model)
        {
            if (model.DeclarationModel.IsObjectReference())
                return elementBuilder.CreateToken(store, model);

            if (!model.IsGetter)
            {
                var ui = new SetVariableNode();
                ui.AddToClassList(Unity.Modifier.GraphElements.Node.k_UssClassName + "--setvar-node");
                ui.Setup(model, store, elementBuilder.GraphView);
                return ui;
            }

            var token = new Token();
            token.AddToClassList("dots-variable-token");
            token.Setup(model, store, elementBuilder.GraphView);
            return token;
        }

        public static IGraphElement CreateDotsNode(this ElementBuilder elementBuilder, IStore store, BaseDotsNodeModel model)
        {
            var ui = new DotsNode();
            ui.Setup(model, store, elementBuilder.GraphView);
            return ui;
        }
    }
}
