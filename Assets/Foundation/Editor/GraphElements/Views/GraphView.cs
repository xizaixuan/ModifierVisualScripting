
using System;
using System.Collections.Generic;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public struct GraphViewChange
    {
        // Operations Pending
        public List<GraphElement> elementsToRemove;
        public List<Edge> edgeToCreate;

        // Operations Completed
        public List<GraphElement> moveElements;
        public Vector2 moveDelta;
    }


    public struct NodeCreationContext
    {
        public Vector2 screenMousePosition;
        public VisualElement target;
        public int index;
    }

    public abstract class GraphView : GraphViewBridge
    {
        // Layer class. Used for queries below
        public class Layer : VisualElement {}

        // Delegates and Callbacks
        public Action<NodeCreationContext> nodeCreationRequest { get; set; }

        internal IInsertLocation currentInsertLocation { get; set; }
    }
}