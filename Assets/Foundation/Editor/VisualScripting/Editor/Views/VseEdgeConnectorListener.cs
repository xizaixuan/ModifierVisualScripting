using System;
using Unity.Modifier.GraphElements;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    class VseEdgeConnectorListener : IEdgeConnectorListener
    {
        readonly Action<Unity.Modifier.GraphElements.Edge, Vector2> m_OnDropOutsideDelegate;
        readonly Action<Unity.Modifier.GraphElements.Edge> m_OnDropDelegate;

        public VseEdgeConnectorListener(Action<Unity.Modifier.GraphElements.Edge, Vector2> onDropOutsideDelegate, Action<Unity.Modifier.GraphElements.Edge> onDropDelegate)
        {
            m_OnDropOutsideDelegate = onDropOutsideDelegate;
            m_OnDropDelegate = onDropDelegate;
        }

        public void OnDropOutsidePort(Unity.Modifier.GraphElements.Edge edge, Vector2 position)
        {
            m_OnDropOutsideDelegate(edge, position);
        }

        public void OnDrop(GraphView graphView, Unity.Modifier.GraphElements.Edge edge)
        {
            m_OnDropDelegate(edge);
        }
    }
}