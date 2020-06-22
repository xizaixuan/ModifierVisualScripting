using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class StackNode : Unity.Modifier.GraphElements.StackNode, ICustomColor, IVSGraphViewObserver, INodeState
    {
        public IStackModel StackModel => NodeModel as IStackModel;
        public new Store Store => base.Store as Store;

        Dictionary<VisualElement, IManipulator> m_StackSeparatorManipulators = new Dictionary<VisualElement, IManipulator>();

        VisualElement m_StackSeparatorContainer;
        VisualElement StackSeparatorContainer => m_StackSeparatorContainer ?? (m_StackSeparatorContainer = this.Q("stackSeparatorContainer"));

        protected override bool hasMultipleSelectionSupport => true;

        public NodeUIState UIState { get; set; }

        const string k_StackNodeSeparatorAddLabel = "stackNodeSeparatorAddLabel";

        public StackNode()
        {
            this.AddManipulator(new ContextualMenuManipulator(OnContextualMenuEvent));
            RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
            RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        protected override void BuildUI()
        {
            base.BuildUI();

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "StackNode.uss"));

            style.overflow = Overflow.Visible;

            var borderItem = this.MandatoryQ("borderItem");
            borderItem.style.overflow = Overflow.Visible;
        }

        public override void UpdateFromModel()
        {
            SetPosition(new Rect(StackModel.Position.x, StackModel.Position.y, 0, 0));

            UpdatePortsFromModels(StackModel.InputPorts, "input");
            UpdatePortsFromModels(StackModel.OutputPorts, "output");

            viewDataKey = StackModel.GetId();

            if (!StackModel.NodeModels.Any())
            {
                var contentContainerPlaceholder = this.MandatoryQ("stackNodeContentContainerPlaceholder");
                contentContainerPlaceholder.AddToClassList("empty");
            }

            PopulateStack(GraphView, Store, StackModel, this);
        }

        void UpdatePortsFromModels(IEnumerable<IPortModel> portModelsToCreate, string portCollectionName)
        {
            VisualElement portsElement = this.MandatoryQ(portCollectionName);

            foreach (IPortModel inputPortModel in portModelsToCreate)
            {
                if (inputPortModel.PortType != PortType.Execution && inputPortModel.PortType != PortType.Loop)
                    continue;

                if (GraphElementFactory.CreateUI<Port>(GraphView, Store, inputPortModel as IGTFPortModel) is Port port)
                {
                    port.Orientation = Orientation.Vertical;
                    portsElement.Add(port);
                }
            }
        }

        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);

            if (evt.eventTypeId == GeometryChangedEvent.TypeId())
            {
                UpdateSeparators();
            }
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            schedule.Execute(a => UpdateSeparators());
        }

        void UpdateSeparators()
        {
            // Add '+' signs to each separator
            if (StackSeparatorContainer == null)
                return;

            foreach (VisualElement stackSeparator in StackSeparatorContainer.Children().Where(x => x.ClassListContains("stack-node-separator")))
            {
                if (!m_StackSeparatorManipulators.ContainsKey(stackSeparator))
                {
                    stackSeparator.pickingMode = PickingMode.Position;
                    var stackSeparatorManipulator = new StackSeparatorManipulator(OnSeparatorMouseUp);
                    stackSeparator.AddManipulator(stackSeparatorManipulator);
                    m_StackSeparatorManipulators[stackSeparator] = stackSeparatorManipulator;
                }

                var highlight = stackSeparator.MandatoryQ("highlight");

                bool foundAddLabel = false;
                foreach (VisualElement stackNodeSeparatorChild in highlight.Children())
                {
                    if (stackNodeSeparatorChild.name != k_StackNodeSeparatorAddLabel)
                        continue;
                    foundAddLabel = true;
                    break;
                }

                if (foundAddLabel)
                    continue;

                var addLabel = new Label { name = k_StackNodeSeparatorAddLabel, text = "+" };
                highlight.Add(addLabel);
            }
        }

        void OnSeparatorMouseUp()
        {
            if (GraphView is VseGraphView vseGraphView)
                vseGraphView.window.DisplaySmartSearch();
        }

        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element.ClassListContains("standalone"))
                return false;

            if (!(element is Node node))
                return false;

            // TODO : Use stackModel.AcceptMode instead of AllowAddBranchTypeNode that relies on Node visual style
            // Order is important here, please do not switch conditions
            bool canDrop = ContainsNode(node) ||
                !node.NodeModel.IsBranchType ||
                AcceptNode(node.NodeModel);

            if (canDrop)
            {
                if (node.NodeModel.IsBranchType)
                {
                    proposedIndex = maxIndex;
                }
                else
                {
                    List<Node> children = Children().OfType<Node>().ToList();
                    if (proposedIndex > 0 && proposedIndex == children.Count)
                    {
                        if (children[proposedIndex - 1].NodeModel.IsBranchType)
                        {
                            proposedIndex = children.Count - 1;
                        }
                    }
                }
            }

            return canDrop;
        }

        bool AcceptNode(INodeModel nodeModel)
        {
            return StackModel.NodeModels.Any(m => m == nodeModel) || StackModel.AcceptNode(nodeModel.GetType());
        }

        public bool HasBranchedNode()
        {
            return StackModel.NodeModels.Any(m => m.IsBranchType);
        }

        bool ContainsNode(Node node)
        {
            return Children().OfType<Node>().Any(n => n.NodeModel == node?.NodeModel);
        }

        public override int GetInsertionIndex(Vector2 worldPosition)
        {
            var insertIndex = base.GetInsertionIndex(worldPosition);
            if (HasBranchedNode() && insertIndex == childCount)
                insertIndex--;

            return insertIndex;
        }

        public IGraphElementModel GraphElementModel => StackModel;

        protected virtual void OnEnterPanel(AttachToPanelEvent e)
        {
        }

        protected virtual void OnLeavePanel(DetachFromPanelEvent e)
        {
        }

        protected virtual void OnContextualMenuEvent(ContextualMenuPopulateEvent evt)
        {
            GraphView.BuildContextualMenu(evt);
        }

        static void PopulateStack(GraphView graphView, Store store, IStackModel stackModel, StackNode stack)
        {
            foreach (var nodeModel in stackModel.NodeModels)
            {
                if (GraphElementFactory.CreateUI<GraphElement>(graphView, store, nodeModel as IGTFNodeModel) is GraphElement node)
                {
                    stack.AddElement(node);
                    if (graphView is VseGraphView vseGraphView)
                        vseGraphView.AddPositionDependency(stack.StackModel, nodeModel);
                    node.style.position = Position.Relative;
                }
            }
        }

        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            evt.menu.AppendAction("Create Node", menuAction =>
                ((VseGraphView)GraphView).window.DisplaySmartSearch(menuAction, separatorIndex), DropdownMenuAction.AlwaysEnabled);

            evt.menu.AppendAction("Split Stack", menuAction =>
            {
                Store.Dispatch(new SplitStackAction(StackModel, separatorIndex));
            }, action => separatorIndex == 0 || separatorIndex >= StackModel.NodeModels.Count ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
        }

        public override bool DragExited()
        {
            base.DragExited();
            ((VseGraphView)GraphView).ClearPlaceholdersAfterDrag();
            return false;
        }

        public override void OnStartDragging(GraphElement ge)
        {
            if (ge is TokenDeclaration tokenDeclaration)
            {
                VseGraphView gView = GetFirstOfType<VseGraphView>();
                VisualElement tokenParent = tokenDeclaration.parent;
                int childIndex = tokenDeclaration.FindIndexInParent();

                gView.RemoveElement(tokenDeclaration);
                tokenDeclaration.SetClasses();
                gView.AddElement(tokenDeclaration);
                // By removing the tokenDeclaration from the graph view, we also removed it from the selection.
                // We need to add the tokenDeclaration back in order to properly drag.
                tokenDeclaration.Select(gView, true);
                tokenDeclaration.BringToFront();

                TokenDeclaration placeHolderTokenDeclaration = tokenDeclaration.Clone();
                placeHolderTokenDeclaration.AddToClassList("placeHolder");
                tokenParent.Insert(childIndex, placeHolderTokenDeclaration);
                placeHolderTokenDeclaration.MarkDirtyRepaint();

                gView.AddPlaceholderToken(tokenDeclaration);
            }
            else
            {
                var originalWidth = ge.resolvedStyle.width;
                var originalHeight = ge.resolvedStyle.height;
                base.OnStartDragging(ge);
                // Revert to same width and height after element became unstacked
                ge.style.width = originalWidth;
                ge.style.height = originalHeight;
            }
        }

        public void OnAddedToGraphView()
        {
            this.Query<GraphElement>().ForEach(childGraphElement =>
            {
                switch (childGraphElement)
                {
                    case TokenDeclaration tokenDeclaration:
                        ((VseGraphView)GraphView).RestoreSelectionForElement(tokenDeclaration);
                        break;
                    case IHasGraphElementModel hasGraphElementChildModel when Store.GetState().EditorDataModel
                        .ShouldSelectElementUponCreation(hasGraphElementChildModel):
                        childGraphElement.Select(GraphView, true);
                        break;
                    case IVisualScriptingField visualScriptingField when Store.GetState().EditorDataModel
                        .ShouldExpandElementUponCreation(visualScriptingField):
                        visualScriptingField.Expand();
                        break;
                }
            });
        }
    }

    class StackSeparatorManipulator : MouseManipulator
    {
        bool m_Active;
        readonly Action m_OnMouseUp;
        Vector2 m_ActivationPosition;
        const float k_ActivationPositionThreshold = 4f;

        public StackSeparatorManipulator(Action onMouseUp)
        {
            m_OnMouseUp = onMouseUp;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        void OnMouseDown(MouseDownEvent e)
        {
            if (!CanStartManipulation(e))
                return;
            m_ActivationPosition = e.mousePosition;
            m_Active = true;
            e.StopPropagation();
        }

        void OnMouseUp(MouseUpEvent e)
        {
            if (!m_Active)
                return;

            Vector2 mousePosition = e.mousePosition;
            if (Mathf.Abs(mousePosition.x - m_ActivationPosition.x) < k_ActivationPositionThreshold &&
                Mathf.Abs(mousePosition.y - m_ActivationPosition.y) < k_ActivationPositionThreshold)
                m_OnMouseUp.Invoke();

            m_Active = false;
            e.StopPropagation();
        }
    }
}