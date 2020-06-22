using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.Editor.ConstantEditor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using ISelectable = Unity.Modifier.GraphElements.ISelectable;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class Port : Unity.Modifier.GraphElements.Port, IDropTarget, IBadgeContainer
    {
        const string k_DropHighlightClass = "drop-highlighted";

        VisualElement m_InputEditor; // if this port allows editing an input, holds the element editing it

        VseGraphView VseGraphView => GraphView as VseGraphView;
        public new Store Store => base.Store as Store;
        public new PortModel PortModel => base.PortModel as PortModel;

        public IconBadge ErrorBadge { get; set; }
        public ValueBadge ValueBadge { get; set; }

        /// <summary>
        /// Used to highlight the port when it is triggered during tracing
        /// </summary>
        public bool ExecutionPortActive
        {
            get => ClassListContains("execution-active");
            set => EnableInClassList("execution-active", value);
        }

        static void OnDropOutsideCallback(IStore store, Vector2 pos, Unity.Modifier.GraphElements.Edge edge)
        {
            VseGraphView graphView = edge.GetFirstAncestorOfType<VseGraphView>();
            Vector2 localPos = graphView.contentViewContainer.WorldToLocal(pos);

            List<IGTFEdgeModel> edgesToDelete = EdgeConnectorListener.GetDropEdgeModelsToDelete(edge.EdgeModel);

            // when grabbing an existing edge's end, the edgeModel should be deleted
            if (edge.EdgeModel != null)
                edgesToDelete.Add(edge.EdgeModel);

            IStackModel targetStackModel = null;
            int targetIndex = -1;
            StackNode stackNode = graphView.lastHoveredVisualElement as StackNode ??
                graphView.lastHoveredVisualElement.GetFirstOfType<StackNode>();

            if (stackNode != null)
            {
                targetStackModel = stackNode.StackModel;
                targetIndex = stackNode.GetInsertionIndex(pos);
            }

            IPortModel existingPortModel;
            // warning: when dragging the end of an existing edge, both ports are non null.
            if (edge.Input != null && edge.Output != null)
            {
                float distanceToOutput = Vector2.Distance(edge.EdgeControl.from, pos);
                float distanceToInput = Vector2.Distance(edge.EdgeControl.to, pos);
                // note: if the user was able to stack perfectly both ports, we'd be in trouble
                if (distanceToOutput < distanceToInput)
                    existingPortModel = edge.Input as IPortModel;
                else
                    existingPortModel = edge.Output as IPortModel;
            }
            else
            {
                var existingPort = (edge.Input ?? edge.Output);
                existingPortModel = existingPort as IPortModel;
            }

            ((Store)store)?.GetState().CurrentGraphModel?.Stencil.CreateNodesFromPort((Store)store, existingPortModel, localPos, pos, edgesToDelete, targetStackModel, targetIndex);
        }

        public Port()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            if (m_Icon != null)
                m_Icon.tintColor = PortColor;
        }

        Image m_Icon;

        protected override void BuildUI()
        {
            base.BuildUI();

            if (ConnectorBox != null)
            {
                m_Icon = new Image();
                m_Icon.AddToClassList(k_UssClassName + "__icon");
                m_Icon.tintColor = PortColor;
                var connectorBoxIndex = ConnectorBox.parent.IndexOf(ConnectorBox);
                ConnectorBox.parent.Insert(connectorBoxIndex + 1, m_Icon);
            }

            BuildConstantEditor();

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "Port.uss"));
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "PropertyField.uss"));

            m_EdgeConnector?.SetDropOutsideDelegate((s, edge, pos) => OnDropOutsideCallback(s, pos, edge));
        }

        static readonly string k_PortTypeClassNamePrefix = "ge-port--type-";
        static string GetClassNameForType(PortType t)
        {
            return k_PortTypeClassNamePrefix + t.ToString().ToLower();
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            this.PrefixRemoveFromClassList(k_PortTypeClassNamePrefix);
            AddToClassList(GetClassNameForType(PortModel.PortType));

            if (m_InputEditor != null)
            {
                m_InputEditor.RemoveFromHierarchy();
                m_InputEditor = null;
                BuildConstantEditor();
            }
        }

        void BuildConstantEditor()
        {
            if (PortModel.Direction == Direction.Input && PortModel.EmbeddedValue != null)
            {
                var embeddedValueEditorValueChangedOverride = PortModel.EmbeddedValueEditorValueChangedOverride;
                var localInputPortModel = PortModel;
                VisualElement editor = this.CreateEditorForNodeModel(PortModel.EmbeddedValue, embeddedValueEditorValueChangedOverride != null ? new Action<IChangeEvent>(x => embeddedValueEditorValueChangedOverride.Invoke(x, Store, localInputPortModel)) : (_ => Store.Dispatch(new RefreshUIAction(UpdateFlags.RequestCompilation))));
                if (editor != null)
                {
                    Add(editor);
                    m_InputEditor = editor;
                    m_InputEditor.SetEnabled(!PortModel.IsConnected || PortModel.ConnectionPortModels.All(p => p.NodeModel.State == ModelState.Disabled));
                }
            }
        }

        public bool CanAcceptDrop(List<ISelectable> dragSelection)
        {
            return dragSelection.Count == 1 &&
                (PortModel.PortType != PortType.Execution &&
                    (dragSelection[0] is IVisualScriptingField
                        || dragSelection[0] is TokenDeclaration
                        || IsTokenToDrop(dragSelection[0])));
        }

        bool IsTokenToDrop(ISelectable selectable)
        {
            return selectable is Token token
                && token.GraphElementModel is IVariableModel varModel
                && !varModel.OutputPort.ConnectionPortModels.Any(p => p == PortModel)
                && PortModel.NodeModel != token.GraphElementModel;
        }

        public bool DragUpdated(DragUpdatedEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource)
        {
            return true;
        }

        public bool DragPerform(DragPerformEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource)
        {
            if (GraphView == null)
                return false;

            List<ISelectable> selectionList = selection.ToList();
            List<GraphElement> dropElements = selectionList.OfType<GraphElement>().ToList();

            Assert.IsTrue(dropElements.Count == 1);

            var edgesVMToDelete = PortModel.Capacity == PortCapacity.Multi ? new List<IEdgeModel>() : PortModel.ConnectedEdges;
            var edgesToDelete = edgesVMToDelete;

            if (IsTokenToDrop(selectionList[0]))
            {
                Token token = ((Token)selectionList[0]);
                token.SetMovable(true);
                Store.Dispatch(new CreateEdgeAction(PortModel, ((IVariableModel)token.GraphElementModel).OutputPort as IGTFPortModel, edgesToDelete.Cast<IGTFEdgeModel>(), CreateEdgeAction.PortAlignmentType.Input));
                return true;
            }

            List<Tuple<IVariableDeclarationModel, Vector2>> variablesToCreate = DragAndDropHelper.ExtractVariablesFromDroppedElements(dropElements, VseGraphView, evt.mousePosition);

            PortType type = PortModel.PortType;
            if (type != PortType.Data && type != PortType.Instance) // do not connect loop/exec ports to variables
            {
                return VseGraphView.DragPerform(evt, selectionList, dropTarget, dragSource);
            }

            IVariableDeclarationModel varModelToCreate = variablesToCreate.Single().Item1;

            Store.Dispatch(new CreateVariableNodesAction(varModelToCreate, evt.mousePosition, edgesToDelete.Cast<IGTFEdgeModel>(), PortModel, autoAlign: true));

            VseGraphView.ClearPlaceholdersAfterDrag();

            return true;
        }

        public bool DragEnter(DragEnterEvent evt, IEnumerable<ISelectable> selection, IDropTarget enteredTarget, ISelection dragSource)
        {
            AddToClassList(k_DropHighlightClass);
            var dragSelection = selection.ToList();
            if (dragSelection.Count == 1 && dragSelection[0] is Token token)
                token.SetMovable(false);
            return true;
        }

        public bool DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> selection, IDropTarget leftTarget, ISelection dragSource)
        {
            RemoveFromClassList(k_DropHighlightClass);
            var dragSelection = selection.ToList();
            if (dragSelection.Count == 1 && dragSelection[0] is Token token)
                token.SetMovable(true);
            return true;
        }

        public bool DragExited()
        {
            RemoveFromClassList(k_DropHighlightClass);
            VseGraphView?.ClearPlaceholdersAfterDrag();
            return false;
        }
    }
}