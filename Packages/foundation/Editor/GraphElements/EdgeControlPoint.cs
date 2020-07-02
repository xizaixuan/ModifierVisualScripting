using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class EdgeControlPoint : VisualElement
    {
        static readonly string k_ClassName = "edge-control-point";

        EdgeControl m_EdgeControl;
        IGTFEdgeModel m_EdgeModel;
        int m_ControlPointIndex;

        public EdgeControlPoint(EdgeControl edgeControl, IGTFEdgeModel edgeModel, int controlPointIndex)
        {
            m_EdgeControl = edgeControl;
            m_EdgeModel = edgeModel;
            m_ControlPointIndex = controlPointIndex;

            AddToClassList(k_ClassName);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);

            style.position = Position.Absolute;
        }

        bool m_DraggingControlPoint;
        bool m_DraggingTightness;
        Vector2 m_OriginalElementPosition;
        float m_OriginalTightness;
        Vector2 m_OriginalPointerPosition;

        void OnPointerDown(PointerDownEvent e)
        {
            if (!e.isPrimary || e.button != 0)
                return;

            m_OriginalPointerPosition = this.ChangeCoordinatesTo(parent, e.localPosition);
            m_OriginalElementPosition = m_EdgeModel.EdgeControlPoints[m_ControlPointIndex].Position;
            m_OriginalTightness = m_EdgeModel.EdgeControlPoints[m_ControlPointIndex].Tightness;

            if (e.modifiers == EventModifiers.None)
            {
                m_DraggingControlPoint = true;
            }
            else if (e.modifiers == EventModifiers.Alt)
            {
                m_DraggingTightness = true;
            }

            if (m_DraggingControlPoint || m_DraggingTightness)
            {
                this.CapturePointer(e.pointerId);
                e.StopPropagation();
            }
        }

        void OnPointerMove(PointerMoveEvent e)
        {
            GraphView graphView = null;
            Vector2 pointerDelta = Vector2.zero;
            if (m_DraggingControlPoint || m_DraggingTightness)
            {
                graphView = GetFirstAncestorOfType<GraphView>();
                var pointerPosition = this.ChangeCoordinatesTo(parent, e.localPosition);
                pointerDelta = new Vector2(pointerPosition.x, pointerPosition.y) - m_OriginalPointerPosition;
            }

            if (graphView == null)
            {
                return;
            }

            if (m_DraggingControlPoint)
            {
                var newPosition = m_OriginalElementPosition + pointerDelta;
                graphView.Store.Dispatch(new MoveEdgeControlPointAction(m_EdgeModel, m_ControlPointIndex, newPosition, m_OriginalTightness));
                m_EdgeControl.MarkDirtyRepaint();
                e.StopPropagation();
            }
            else if (m_DraggingTightness)
            {
                var tightnessDelta = pointerDelta.x - pointerDelta.y;
                var newTightness = m_OriginalTightness + tightnessDelta;
                graphView.Store.Dispatch(new MoveEdgeControlPointAction(m_EdgeModel, m_ControlPointIndex, m_OriginalElementPosition, newTightness));
                e.StopPropagation();
                m_EdgeControl.MarkDirtyRepaint();
            }
        }

        void OnPointerUp(PointerUpEvent e)
        {
            if (!e.isPrimary || e.button != 0)
                return;

            this.ReleasePointer(e.pointerId);
            m_DraggingControlPoint = false;
            m_DraggingTightness = false;
            e.StopPropagation();
        }

        public void SetPositions(Vector2 cpPosition, Vector2 lhPosition, Vector2 rhPosition)
        {
            style.left = cpPosition.x;
            style.top = cpPosition.y;
        }
    }
}