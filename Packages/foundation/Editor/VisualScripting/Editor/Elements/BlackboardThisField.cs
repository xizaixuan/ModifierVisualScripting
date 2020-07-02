using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor.Highlighting;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class BlackboardThisField : BlackboardField, IHighlightable, IVisualScriptingField, IHasGraphElementModel
    {
        public IGraphElementModel GraphElementModel => Model as IGraphElementModel;
        public IGraphElementModel ExpandableGraphElementModel => null;

        public void Expand() { }
        public bool CanInstantiateInGraph() => true;

        public bool Highlighted
        {
            get => highlighted;
            set => highlighted = value;
        }

        public BlackboardThisField(VseGraphView graphView, ThisNodeModel nodeModel, IGraphModel graphModel)
        {
            Setup(nodeModel, null, graphView);

            text = "This";

            var pill = this.MandatoryQ<Pill>("pill");
            pill.tooltip = text;

            typeText = graphModel?.FriendlyScriptName;

            viewDataKey = "blackboardThisFieldKey";
        }

        public override bool IsRenamable()
        {
            return false;
        }

        public bool ShouldHighlightItemUsage(IGraphElementModel candidate)
        {
            return candidate is ThisNodeModel;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            (GraphView as VseGraphView).HighlightGraphElements();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            (GraphView as VseGraphView).ClearGraphElementsHighlight(ShouldHighlightItemUsage);
        }
    }
}