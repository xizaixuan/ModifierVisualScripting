﻿using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class EdgeConnector : MouseManipulator
    {
        readonly EdgeConnectorListener m_EdgeConnectorListener;
        readonly EdgeDragHelper m_EdgeDragHelper;
        bool m_Active;
        Vector2 m_MouseDownPosition;

        internal const float k_ConnectionDistanceThreshold = 10f;

        public EdgeConnector(IStore store, GraphView graphView, EdgeConnectorListener listener, Func<IGTFGraphModel, GhostEdgeModel> ghostEdgeViewModelCreator = null)
        {
            m_EdgeConnectorListener = listener;
            m_EdgeDragHelper = new EdgeDragHelper(store, graphView, listener, ghostEdgeViewModelCreator);
            m_Active = false;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        public virtual EdgeDragHelper edgeDragHelper => m_EdgeDragHelper;

        public void SetDropOutsideDelegate(Action<IStore, Edge, Vector2> action)
        {
            m_EdgeConnectorListener.SetDropOutsideDelegate(action);
        }

        public void SetDropDelegate(Action<IStore, Edge> action)
        {
            m_EdgeConnectorListener.SetDropDelegate(action);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<MouseCaptureOutEvent>(OnCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected virtual void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (!CanStartManipulation(e))
            {
                return;
            }

            var port = target.GetFirstAncestorOfType<Port>();
            if (port == null)
            {
                return;
            }

            m_MouseDownPosition = e.localMousePosition;

            m_EdgeDragHelper.CreateEdgeCandidate(port.PortModel.GraphModel);
            m_EdgeDragHelper.draggedPort = port.PortModel;

            if (m_EdgeDragHelper.HandleMouseDown(e))
            {
                m_Active = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
            else
            {
                m_EdgeDragHelper.Reset();
            }
        }

        void OnCaptureOut(MouseCaptureOutEvent e)
        {
            m_Active = false;
            if (m_EdgeDragHelper.edgeCandidateModel != null)
                Abort();
        }

        protected virtual void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active) return;
            m_EdgeDragHelper.HandleMouseMove(e);
            e.StopPropagation();
        }

        protected virtual void OnMouseUp(MouseUpEvent e)
        {
            if (!m_Active || !CanStopManipulation(e))
                return;

            try
            {
                if (CanPerformConnection(e.localMousePosition))
                    m_EdgeDragHelper.HandleMouseUp(e);
                else
                    Abort();
            }
            finally
            {
                m_Active = false;
                target.ReleaseMouse();
                e.StopPropagation();
            }
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.Escape || !m_Active)
                return;

            Abort();

            m_Active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }

        void Abort()
        {
            m_EdgeDragHelper.Reset();
        }

        bool CanPerformConnection(Vector2 mousePosition)
        {
            return Vector2.Distance(m_MouseDownPosition, mousePosition) > k_ConnectionDistanceThreshold;
        }
    }
}