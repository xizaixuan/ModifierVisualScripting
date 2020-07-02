﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphToolsFoundation.Model;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class Placemat : GraphElement, IResizable, IMovable, IDropTarget
    {
        internal static readonly Vector2 k_DefaultCollapsedSize = new Vector2(200, 42);
        static readonly int k_SelectRectOffset = 3;

        public IGTFPlacematModel PlacematModel => Model as IGTFPlacematModel;

        TextField m_TitleField;
        ResizableElement m_Resizer;
        Button m_CollapseButton;

        protected ContextualMenuManipulator m_ContextualMenuManipulator;

        public Placemat()
        {
            focusable = true;
            usageHints = UsageHints.DynamicTransform;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            m_ContextualMenuManipulator = new ContextualMenuManipulator(BuildContextualMenu);
            this.AddManipulator(m_ContextualMenuManipulator);
        }

        protected override void BuildUI()
        {
            base.BuildUI();

            GraphElementsHelper.LoadTemplateAndStylesheet(this, "PlacematElement", "ge-placemat");

            AddToClassList("placemat");
            AddToClassList("selectable");

            m_TitleField = this.Q<TextField>();
            m_TitleField.isDelayed = true;

            m_CollapseButton = this.Q<Button>();
            m_CollapseButton.clicked += () => Collapsed = !Collapsed;
            m_CollapseButton.AddToClassList("placematCollapse");

            m_Resizer = this.Q<ResizableElement>();

            // PF: Fix this: Placemats are automatically added whereas other elements need to be added manually
            // with GraphView.AddElement. Furthermore, calling GraphView.AddElement(placemat) will remove it
            // from the placemat container and add it to the layer 0.
            GraphView.placematContainer.AddPlacemat(this);
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            if (m_TitleField != null)
                m_TitleField.value = PlacematModel.Title;

            style.backgroundColor = PlacematModel.Color;

            var newPos = PlacematModel.PositionAndSize.position;
            style.left = newPos.x;
            style.top = newPos.y;

            CollapseSelf();

            if (PlacematModel.Collapsed)
            {
                var collapsedElements = new List<GraphElement>();
                if (PlacematModel.HiddenElements != null)
                {
                    foreach (var elementModel in PlacematModel.HiddenElements)
                    {
                        var graphElement = elementModel.GetUI<GraphElement>(GraphView);
                        if (graphElement != null)
                            collapsedElements.Add(graphElement);
                    }
                }

                GatherCollapsedEdges(collapsedElements);
                CollapsedElements = collapsedElements;
            }
            else
            {
                CollapsedElements = null;
            }
        }

        public int ZOrder => PlacematModel.ZOrder;

        internal Vector2 UncollapsedSize => PlacematModel.PositionAndSize.size;

        Vector2 CollapsedSize
        {
            get
            {
                var actualCollapsedSize = k_DefaultCollapsedSize;
                if (UncollapsedSize.x < k_DefaultCollapsedSize.x)
                    actualCollapsedSize.x = UncollapsedSize.x;

                return actualCollapsedSize;
            }
        }

        Rect ExpandedPosition => Collapsed ? new Rect(layout.position, UncollapsedSize) : layout;

        public override void SetPosition(Rect newPos)
        {
            if (Collapsed)
                newPos.size = CollapsedSize;

            base.SetPosition(newPos);
            style.height = newPos.height;
            style.width = newPos.width;
        }

        PlacematContainer m_PlacematContainer;

        PlacematContainer Container =>
            m_PlacematContainer ?? (m_PlacematContainer = GetFirstAncestorOfType<PlacematContainer>());

        HashSet<GraphElement> m_CollapsedElements = new HashSet<GraphElement>();

        IEnumerable<GraphElement> CollapsedElements
        {
            get => m_CollapsedElements;
            set
            {
                foreach (var collapsedElement in m_CollapsedElements)
                {
                    collapsedElement.style.visibility = StyleKeyword.Null;
                }

                foreach (var node in AllCollapsedElements(m_CollapsedElements).OfType<Node>())
                {
                    node.MarkEdgesDirty();
                }

                m_CollapsedElements.Clear();

                if (value == null)
                    return;

                foreach (var collapsedElement in value)
                {
                    collapsedElement.style.visibility = Visibility.Hidden;
                    m_CollapsedElements.Add(collapsedElement);
                }

                foreach (var node in AllCollapsedElements(m_CollapsedElements).OfType<Node>())
                {
                    node.MarkEdgesDirty();
                }
            }
        }

        static IEnumerable<GraphElement> AllCollapsedElements(IEnumerable<GraphElement> collapsedElements)
        {
            if (collapsedElements != null)
            {
                foreach (var element in collapsedElements)
                {
                    switch (element)
                    {
                        case Placemat placemat when placemat.Collapsed:
                            {
                                // TODO: evaluate performance of this recursive call.
                                foreach (var subElement in AllCollapsedElements(placemat.CollapsedElements))
                                    yield return subElement;
                                yield return element;
                                break;
                            }
                        case Placemat placemat when !placemat.Collapsed:
                            yield return element;
                            break;
                        case Node nodeModel:
                            yield return nodeModel;
                            break;
                    }
                }
            }
        }

        public bool Collapsed
        {
            get => PlacematModel.Collapsed;
            set
            {
                if (PlacematModel.Collapsed != value)
                {
                    var collapsedModels = value ? GatherCollapsedElements() : null;
                    Store.Dispatch(new ExpandOrCollapsePlacematAction(value, collapsedModels, PlacematModel));
                }
            }
        }

        void CollapseSelf()
        {
            if (Collapsed)
            {
                style.width = CollapsedSize.x;
                style.height = CollapsedSize.y;

                if (m_Resizer != null)
                    m_Resizer.style.visibility = Visibility.Hidden;
            }
            else
            {
                style.width = UncollapsedSize.x;
                style.height = UncollapsedSize.y;

                if (m_Resizer != null)
                    m_Resizer.style.visibility = StyleKeyword.Null;
            }
            m_CollapseButton?.EnableInClassList("icon-expanded", !Collapsed);
            EnableInClassList("collapsed", Collapsed);
        }

        void GatherCollapsedEdges(List<GraphElement> collapsedElements)
        {
            var allCollapsedNodes = AllCollapsedElements(collapsedElements).OfType<Node>().Select(e => e.NodeModel).ToList();
            foreach (var edge in GraphView.edges.ToList())
                if (AnyNodeIsConnectedToPort(allCollapsedNodes, edge.Input) && AnyNodeIsConnectedToPort(allCollapsedNodes, edge.Output))
                    if (!collapsedElements.Contains(edge))
                        collapsedElements.Add(edge);
        }

        List<IGTFGraphElementModel> GatherCollapsedElements()
        {
            List<GraphElement> collapsedElements = new List<GraphElement>();

            var graphElements = GraphView.graphElements.ToList()
                .Where(e => !(e is Edge) && (e.parent is GraphView.Layer) && IsSelectable())
                .ToList();

            var collapsedElementsElsewhere = new List<GraphElement>();
            RecurseGatherCollapsedElements(this, graphElements, collapsedElementsElsewhere);

            var nodes = new HashSet<IGTFNodeModel>(AllCollapsedElements(collapsedElements).Select(e => e.Model).OfType<IGTFNodeModel>());

            foreach (var edge in GraphView.edges.ToList())
                if (AnyNodeIsConnectedToPort(nodes, edge.Input) && AnyNodeIsConnectedToPort(nodes, edge.Output))
                    collapsedElements.Add(edge);

            foreach (var ge in collapsedElementsElsewhere)
                collapsedElements.Remove(ge);

            return collapsedElements.Select(e => e.Model).ToList();

            void RecurseGatherCollapsedElements(Placemat currentPlacemat, IList<GraphElement> graphElementsParam,
                List<GraphElement> collapsedElementsElsewhereParam)
            {
                var currRect = currentPlacemat.ExpandedPosition;
                var currentActivePlacematRect = new Rect(
                    currRect.x + k_SelectRectOffset,
                    currRect.y + k_SelectRectOffset,
                    currRect.width - 2 * k_SelectRectOffset,
                    currRect.height - 2 * k_SelectRectOffset);
                foreach (var elem in graphElementsParam)
                {
                    if (elem.layout.Overlaps(currentActivePlacematRect))
                    {
                        var placemat = elem as Placemat;
                        if (placemat != null && placemat.ZOrder > currentPlacemat.ZOrder)
                        {
                            if (placemat.Collapsed)
                                foreach (var cge in placemat.CollapsedElements)
                                    collapsedElementsElsewhereParam.Add(cge);
                            else
                                RecurseGatherCollapsedElements(placemat, graphElementsParam, collapsedElementsElsewhereParam);
                        }

                        if (placemat == null || placemat.ZOrder > currentPlacemat.ZOrder)
                            if (elem.resolvedStyle.visibility == Visibility.Visible)
                                collapsedElements.Add(elem);
                    }
                }
            }
        }

        static bool AnyNodeIsConnectedToPort(IEnumerable<IGTFNodeModel> nodes, IGTFPortModel port)
        {
            if (port.NodeModel == null)
            {
                return false;
            }

            foreach (var node in nodes)
            {
                if (node == port.NodeModel)
                    return true;
            }

            return false;
        }

        void OnTitleFieldChange(ChangeEvent<string> evt)
        {
            if (PlacematModel.Title != evt.newValue)
            {
                Store.Dispatch(new ChangePlacematTitleAction(evt.newValue, PlacematModel));
            }
        }

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            var mde = evt as PointerDownEvent;
            if (mde != null)
                if (mde.clickCount == 2 && mde.button == (int)MouseButton.LeftMouse)
                    SelectGraphElementsOver();
        }

        void ActOnGraphElementsOver(Action<GraphElement> act)
        {
            var graphElements = GraphView.graphElements.ToList()
                .Where(e => !(e is Edge) && (e.parent is GraphView.Layer) && IsSelectable());

            foreach (var elem in graphElements)
            {
                if (elem.layout.Overlaps(layout))
                    act(elem);
            }
        }

        internal bool ActOnGraphElementsOver(Func<GraphElement, bool> act, bool includePlacemats)
        {
            var graphElements = GraphView.graphElements.ToList()
                .Where(e => !(e is Edge) && e.parent is GraphView.Layer && IsSelectable()).ToList();

            return RecurseActOnGraphElementsOver_LocalFunc(this, graphElements, act, includePlacemats);
        }

        // TODO: Move to local function of ActOnGraphElementsOver once we move to C# 7.0 or higher.
        static bool RecurseActOnGraphElementsOver_LocalFunc(Placemat currentPlacemat, List<GraphElement> graphElements,
            Func<GraphElement, bool> act, bool includePlacemats)
        {
            if (currentPlacemat.Collapsed)
            {
                foreach (var elem in currentPlacemat.CollapsedElements)
                {
                    var placemat = elem as Placemat;
                    if (placemat != null && placemat.ZOrder > currentPlacemat.ZOrder)
                        if (RecurseActOnGraphElementsOver_LocalFunc(placemat, graphElements, act, includePlacemats))
                            return true;

                    if (placemat == null || (includePlacemats && placemat.ZOrder > currentPlacemat.ZOrder))
                        if (act(elem))
                            return true;
                }
            }
            else
            {
                var currRect = currentPlacemat.ExpandedPosition;
                var currentActivePlacematRect = new Rect(
                    currRect.x + k_SelectRectOffset,
                    currRect.y + k_SelectRectOffset,
                    currRect.width - 2 * k_SelectRectOffset,
                    currRect.height - 2 * k_SelectRectOffset);

                foreach (var elem in graphElements)
                {
                    if (elem.layout.Overlaps(currentActivePlacematRect))
                    {
                        var placemat = elem as Placemat;
                        if (placemat != null && placemat.ZOrder > currentPlacemat.ZOrder)
                            if (RecurseActOnGraphElementsOver_LocalFunc(placemat, graphElements, act, includePlacemats))
                                return true;

                        if (placemat == null || (includePlacemats && placemat.ZOrder > currentPlacemat.ZOrder))
                            if (elem.resolvedStyle.visibility != Visibility.Hidden)
                                if (act(elem))
                                    return true;
                    }
                }
            }
            return false;
        }

        void SelectGraphElementsOver()
        {
            ActOnGraphElementsOver(e => GraphView.AddToSelection(e));
        }

        internal bool WillDragNode(Node node)
        {
            if (Collapsed)
                return AllCollapsedElements(CollapsedElements).Contains(node);

            return ActOnGraphElementsOver(t => node == t, true);
        }

        internal void GrowToFitElements(List<GraphElement> elements)
        {
            if (elements == null)
                elements = GetHoveringNodes();

            var pos = new Rect();
            if (elements.Count > 0 && ComputeElementBounds(ref pos, elements, MinSizePolicy.DoNotEnsureMinSize))
            {
                // We don't resize to be snug. In other words: we don't ever decrease in size.
                Rect currentRect = GetPosition();
                if (pos.xMin > currentRect.xMin)
                    pos.xMin = currentRect.xMin;

                if (pos.xMax < currentRect.xMax)
                    pos.xMax = currentRect.xMax;

                if (pos.yMin > currentRect.yMin)
                    pos.yMin = currentRect.yMin;

                if (pos.yMax < currentRect.yMax)
                    pos.yMax = currentRect.yMax;

                MakeRectAtLeastMinimalSize(ref pos);
                Store.Dispatch(new ChangePlacematPositionAction(pos, ResizeFlags.All, PlacematModel));
            }
        }

        internal void ShrinkToFitElements(List<GraphElement> elements)
        {
            if (elements == null)
                elements = GetHoveringNodes();

            var pos = new Rect();
            if (elements.Count > 0 && ComputeElementBounds(ref pos, elements))
                Store.Dispatch(new ChangePlacematPositionAction(pos, ResizeFlags.All, PlacematModel));
        }

        void ResizeToIncludeSelectedNodes()
        {
            List<GraphElement> nodes = GraphView.selection.OfType<GraphElement>().Where(e => e is Node).ToList();

            // Now include the selected nodes
            var pos = new Rect();
            if (ComputeElementBounds(ref pos, nodes, MinSizePolicy.DoNotEnsureMinSize))
            {
                // We don't resize to be snug: we only resize enough to contain the selected nodes.
                var currentRect = GetPosition();
                if (pos.xMin > currentRect.xMin)
                    pos.xMin = currentRect.xMin;

                if (pos.xMax < currentRect.xMax)
                    pos.xMax = currentRect.xMax;

                if (pos.yMin > currentRect.yMin)
                    pos.yMin = currentRect.yMin;

                if (pos.yMax < currentRect.yMax)
                    pos.yMax = currentRect.yMax;

                MakeRectAtLeastMinimalSize(ref pos);

                Store.Dispatch(new ChangePlacematPositionAction(pos, ResizeFlags.All, PlacematModel));
            }
        }

        internal void GetElementsToMove(bool moveOnlyPlacemat, HashSet<GraphElement> collectedElementsToMove)
        {
            if (Collapsed)
            {
                var collapsedElements = AllCollapsedElements(CollapsedElements);
                foreach (var element in collapsedElements)
                {
                    collectedElementsToMove.Add(element);
                }
            }
            else if (!moveOnlyPlacemat)
            {
                ActOnGraphElementsOver(e =>
                {
                    collectedElementsToMove.Add(e);
                    return false;
                }, true);
            }
        }

        public void OnResized(Rect newRect, ResizeFlags resizeWhat)
        {
            Store.Dispatch(new ChangePlacematPositionAction(newRect, resizeWhat, PlacematModel));
        }

        protected virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var placemat = evt.target as Placemat;

            if (placemat != null)
            {
                evt.menu.AppendAction("Edit Title", a => placemat.StartEditTitle());

                evt.menu.AppendSeparator();

                evt.menu.AppendAction("Change Color...", a =>
                {
                    GraphViewStaticBridge.ShowColorPicker(c =>
                    {
                        if (PlacematModel.Color != c)
                        {
                            Store.Dispatch(new ChangePlacematColorAction(c, PlacematModel));
                        }
                    }, placemat.PlacematModel.Color, false);
                });

                // Resizing section
                evt.menu.AppendSeparator();

                evt.menu.AppendAction(placemat.Collapsed ? "Expand" : "Collapse", a => placemat.Collapsed = !placemat.Collapsed);

                // Gather nodes here so that we don't recycle this code in the resize functions.
                List<GraphElement> hoveringNodes = placemat.GetHoveringNodes();

                evt.menu.AppendAction("Resize/Grow To Fit",
                    a => placemat.GrowToFitElements(hoveringNodes),
                    hoveringNodes.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                evt.menu.AppendAction("Resize/Shrink To Fit",
                    a => placemat.ShrinkToFitElements(hoveringNodes),
                    hoveringNodes.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                evt.menu.AppendAction("Resize/Grow To Fit Selection",
                    a => placemat.ResizeToIncludeSelectedNodes(),
                    s =>
                    {
                        foreach (ISelectable sel in placemat.GraphView.selection)
                        {
                            var node = sel as Node;
                            if (node != null && !hoveringNodes.Contains(node))
                                return DropdownMenuAction.Status.Normal;
                        }

                        return DropdownMenuAction.Status.Disabled;
                    });

                var status = placemat.Container.Placemats.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;

                evt.menu.AppendAction("Order/Bring To Front", a => Container.BringToFront(placemat), status);
                evt.menu.AppendAction("Order/Bring Forward", a => Container.CyclePlacemat(placemat, PlacematContainer.CycleDirection.Up), status);
                evt.menu.AppendAction("Order/Send Backward", a => Container.CyclePlacemat(placemat, PlacematContainer.CycleDirection.Down), status);
                evt.menu.AppendAction("Order/Send To Back", a => Container.SendToBack(placemat), status);
            }
        }

        List<GraphElement> GetHoveringNodes()
        {
            var potentialElements = new List<GraphElement>();
            ActOnGraphElementsOver(e => potentialElements.Add(e));

            return potentialElements.Where(e => e is Node).ToList();
        }

        public void StartEditTitle()
        {
            // Focus field and select text (spare the user from having to type enter before editing text).
            m_TitleField?.Q(TextField.textInputUssName)?.Focus();
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_TitleField.RegisterCallback<ChangeEvent<string>>(OnTitleFieldChange);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            CollapsedElements = null;
            m_TitleField.UnregisterCallback<ChangeEvent<string>>(OnTitleFieldChange);
        }

        internal bool GetPortCenterOverride(IGTFPortModel port, out Vector2 overriddenPosition)
        {
            if (!Collapsed || parent == null)
            {
                overriddenPosition = Vector2.zero;
                return false;
            }

            const int xOffset = 6;
            const int yOffset = 3;
            var halfSize = CollapsedSize * 0.5f;
            var offset = port.Orientation == Orientation.Horizontal
                ? new Vector2(port.Direction == Direction.Input ? -halfSize.x + xOffset : halfSize.x - xOffset, 0)
                : new Vector2(0, port.Direction == Direction.Input ? -halfSize.y + yOffset : halfSize.y - yOffset);

            overriddenPosition = parent.LocalToWorld(layout.center + offset);
            return true;
        }

        // Helper method that calculates how big a Placemat should be to fit the nodes on top of it currently.
        internal const float k_Bounds = 9.0f;
        internal const float k_BoundTop = 29.0f; // Current height of Title

        public enum MinSizePolicy
        {
            EnsureMinSize,
            DoNotEnsureMinSize
        }

        // Returns false if bounds could not be computed.
        public static bool ComputeElementBounds(ref Rect pos, List<GraphElement> elements, MinSizePolicy ensureMinSize = MinSizePolicy.EnsureMinSize)
        {
            if (elements == null || elements.Count == 0)
                return false;

            float minX = Mathf.Infinity;
            float maxX = -Mathf.Infinity;
            float minY = Mathf.Infinity;
            float maxY = -Mathf.Infinity;

            foreach (var r in elements.Select(n => n.GetPosition()))
            {
                if (r.xMin < minX)
                    minX = r.xMin;

                if (r.xMax > maxX)
                    maxX = r.xMax;

                if (r.yMin < minY)
                    minY = r.yMin;

                if (r.yMax > maxY)
                    maxY = r.yMax;
            }

            var width = maxX - minX + k_Bounds * 2.0f;
            var height = maxY - minY + k_Bounds * 2.0f + k_BoundTop;

            pos = new Rect(
                minX - k_Bounds,
                minY - (k_BoundTop + k_Bounds),
                width,
                height);

            if (ensureMinSize == MinSizePolicy.EnsureMinSize)
                MakeRectAtLeastMinimalSize(ref pos);

            return true;
        }

        // The next two values need to be the same as USS... however, we can't get the values from there as we need them in a static
        // methods used to create new placemats
        const float k_MinWidth = 200;
        const float k_MinHeight = 100;

        static void MakeRectAtLeastMinimalSize(ref Rect r)
        {
            if (r.width < k_MinWidth)
                r.width = k_MinWidth;

            if (r.height < k_MinHeight)
                r.height = k_MinHeight;
        }

        public bool CanAcceptDrop(List<ISelectable> dragSelection)
        {
            return (GraphView as IDropTarget)?.CanAcceptDrop(dragSelection) ?? false;
        }

        public bool DragUpdated(DragUpdatedEvent evt, IEnumerable<ISelectable> dragSelection, IDropTarget dropTarget, ISelection dragSource)
        {
            return (GraphView as IDropTarget)?.DragUpdated(evt, dragSelection, dropTarget, dragSource) ?? false;
        }

        public bool DragPerform(DragPerformEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource)
        {
            return (GraphView as IDropTarget)?.DragPerform(evt, selection, dropTarget, dragSource) ?? false;
        }

        public bool DragEnter(DragEnterEvent evt, IEnumerable<ISelectable> dragSelection, IDropTarget enteredTarget, ISelection dragSource)
        {
            return (GraphView as IDropTarget)?.DragEnter(evt, dragSelection, enteredTarget, dragSource) ?? false;
        }

        public bool DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> dragSelection, IDropTarget leftTarget, ISelection dragSource)
        {
            return (GraphView as IDropTarget)?.DragLeave(evt, dragSelection, leftTarget, dragSource) ?? false;
        }

        public bool DragExited()
        {
            return (GraphView as IDropTarget)?.DragExited() ?? false;
        }

        public virtual void UpdatePinning()
        {
        }

        public virtual bool IsMovable => true;
    }
}