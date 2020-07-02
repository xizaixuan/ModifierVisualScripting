using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEngine.UIElements;
using Node = UnityEditor.Modifier.VisualScripting.Editor.Node;

namespace Modifier.DotsStencil
{
    class SmartObjectReferenceNode : Node
    {
        public SmartObjectReferenceNode()
        {
            var clickable = new Clickable(DoAction);
            clickable.activators.Clear();
            clickable.activators.Add(
                new ManipulatorActivationFilter {button = MouseButton.LeftMouse, clickCount = 2});
            this.AddManipulator(clickable);
        }

        void DoAction()
        {
            var graphReferenceNodeModel = (IGraphReferenceNodeModel)Model;
            if (graphReferenceNodeModel.GraphReference != null)
                Store.Dispatch(new LoadGraphAssetAction(
                    graphReferenceNodeModel.GraphReference.GraphModel.GetAssetPath(), graphReferenceNodeModel.GetBoundObject(Store.GetState().EditorDataModel), false, LoadGraphAssetAction.Type.PushOnStack));
        }
    }
}
