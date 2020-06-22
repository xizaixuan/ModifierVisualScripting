using Modifier.VisualScripting.Editor.Elements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    class Edge : Unity.Modifier.GraphElements.Edge, IHasGraphElementModel
    {
        EdgeBubble m_EdgeBubble;

        public EdgeModel VSEdgeModel => EdgeModel as EdgeModel;
        public IGraphElementModel GraphElementModel => VSEdgeModel;

        // Necessary for EdgeConnector, which creates temporary edges
        public Edge()
        {
            layer = -1;

            RegisterCallback<AttachToPanelEvent>(OnTargetAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnTargetDetachedFromPanel);
        }

        protected override void BuildUI()
        {
            base.BuildUI();

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "Edge.uss"));
            m_EdgeBubble = new EdgeBubble();

            PortType portType = VSEdgeModel?.OutputPortModel?.PortType ?? PortType.Data;
            EnableInClassList("execution", portType == PortType.Execution || portType == PortType.Loop);
            EnableInClassList("event", portType == PortType.Event);

            viewDataKey = VSEdgeModel?.GetId();
        }

        void OnTargetAttachedToPanel(AttachToPanelEvent evt)
        {
            Add(m_EdgeBubble);

            if (VSEdgeModel?.OutputPortModel != null)
                VSEdgeModel.OutputPortModel.OnValueChanged += OnPortValueChanged;
        }

        void OnTargetDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (VSEdgeModel?.OutputPortModel != null)
                // ReSharper disable once DelegateSubtraction
                VSEdgeModel.OutputPortModel.OnValueChanged -= OnPortValueChanged;

            m_EdgeBubble.Detach();
            m_EdgeBubble.RemoveFromHierarchy();
        }

        void OnPortValueChanged()
        {
            OnPortChanged();
        }

        public override bool UpdateEdgeControl()
        {
            schedule.Execute(_ => UpdateEdgeBubble());
            return base.UpdateEdgeControl();
        }

        public override void OnPortChanged()
        {
            base.OnPortChanged();

            // Function can be called on initialization from GraphView before the element is attached to a panel
            if (panel == null)
                return;

            UpdateEdgeBubble();
        }

        void UpdateEdgeBubble()
        {
            NodeModel inputPortNodeModel = VSEdgeModel?.InputPortModel?.NodeModel as NodeModel;
            NodeModel outputPortNodeModel = VSEdgeModel?.OutputPortModel?.NodeModel as NodeModel;

            PortType portType = VSEdgeModel?.OutputPortModel?.PortType ?? PortType.Data;
            if ((portType == PortType.Execution || portType == PortType.Loop) && (outputPortNodeModel != null || inputPortNodeModel != null) &&
                !string.IsNullOrEmpty(VSEdgeModel?.EdgeLabel) &&
                visible)
            {
                var offset = EdgeControl.BubblePosition - new Vector2(EdgeControl.layout.xMin + EdgeControl.layout.width / 2, EdgeControl.layout.yMin + EdgeControl.layout.height / 2);
                m_EdgeBubble.SetAttacherOffset(offset);
                m_EdgeBubble.text = VSEdgeModel?.EdgeLabel;
                m_EdgeBubble.EnableInClassList("candidate", Output == null || Input == null);
                m_EdgeBubble.AttachTo(EdgeControl, SpriteAlignment.Center);
                m_EdgeBubble.style.visibility = StyleKeyword.Null;
            }
            else
            {
                m_EdgeBubble.Detach();
                m_EdgeBubble.style.visibility = Visibility.Hidden;
            }
        }
    }
}