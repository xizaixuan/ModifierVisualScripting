using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Searcher;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [PublicAPI]
    public class Node : CollapsiblePortNode, IDroppable, IHighlightable,
        IBadgeContainer, ICustomColor, ICustomSearcherHandler, INodeState
    {
        int m_SelectedIndex;
        public int selectedIndex => m_SelectedIndex;

        public bool InstantAdd { get; set; }

        public StackNode Stack => GetFirstAncestorOfType<StackNode>();

        bool HasInstancePort => m_InstancePort != null;

        public IconBadge ErrorBadge { get; set; }
        public ValueBadge ValueBadge { get; set; }
        public NodeUIState UIState { get; set; }

        protected Port m_InstancePort;

        readonly VisualElement m_InsertLoopPortContainer;

        ProgressBar m_CoroutineProgressBar;
        const float k_ByteToPercentFactor = 100 / 255.0f;
        public byte Progress
        {
            set
            {
                if (m_CoroutineProgressBar != null)
                {
                    m_CoroutineProgressBar.value = value * k_ByteToPercentFactor;
                }
            }
        }

        public new Store Store => base.Store as Store;
        public new NodeModel NodeModel => base.NodeModel as NodeModel;

        VisualElement m_ContentContainer;
        public override VisualElement contentContainer => m_ContentContainer ?? this;

        public Node()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        protected override void BuildUI()
        {
            var selectionBorder = new VisualElement();
            selectionBorder.AddToClassList("ge-node__selection-border");
            Add(selectionBorder);

            var contentContainerElement = new VisualElement();
            contentContainerElement.AddToClassList("ge-node__content-container");
            selectionBorder.Add(contentContainerElement);
            m_ContentContainer = contentContainerElement;

            base.BuildUI();

            if (TitleContainer != null)
            {
                // Add an icon and wrap the icon and the title label.

                var iconAndTitleWrapper = new VisualElement();
                iconAndTitleWrapper.AddToClassList("ge-node__icon-title-wrapper");

                var icon = new VisualElement();
                icon.AddToClassList(k_UssClassName + "__icon");
                icon.AddToClassList(NodeModel.IconTypeString);
                iconAndTitleWrapper.Insert(0, icon);

                var tcIndex = TitleContainer.IndexOf(TitleLabel);
                TitleContainer.Insert(tcIndex, iconAndTitleWrapper);
                iconAndTitleWrapper.Add(TitleLabel);
            }

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "Node.uss"));

            if (NodeModel.HasProgress && TitleContainer != null)
            {
                m_CoroutineProgressBar = new ProgressBar();
                m_CoroutineProgressBar.AddToClassList(k_UssClassName + "__progress-bar");
                TitleContainer.Insert(1, m_CoroutineProgressBar);
            }

            this.AddOverlay();
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            AddToClassList(NodeModel.ParentStackModel == null ? "standalone" : "stackable-node");

            viewDataKey = NodeModel.GetId();

            m_CoroutineProgressBar?.EnableInClassList("hidden", !NodeModel.HasProgress);

            EnableInClassList("has-instance-port", HasInstancePort);

            if (NodeModel is IObjectReference modelReference)
            {
                EnableInClassList("invalid", modelReference.ReferencedObject == null);
            }

            if (TitleContainer != null)
            {
                if (NodeModel.HasUserColor)
                {
                    TitleContainer.style.backgroundColor = NodeModel.Color;
                }
                else
                {
                    TitleContainer.style.backgroundColor = StyleKeyword.Null;
                }
            }

            UIState = NodeModel.State == ModelState.Disabled ? NodeUIState.Disabled : NodeUIState.Enabled;
            this.ApplyNodeState();

            tooltip = NodeModel.ToolTip;
        }

        public override void UpdatePinning()
        {
            m_SelectedIndex = -1;
        }

        public override bool IsMovable => ClassListContains("standalone");

        public bool IsInStack => !(ClassListContains("standalone"));

        public int FindIndexInStack()
        {
            // Find index of child so we can provide with the correct information
            var nodes = Stack?.Query<Node>().ToList();
            // uQuery can return null lists...
            if (nodes != null)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    if (this == nodes[i])
                        return i;
                }
            }

            return -1;
        }

        void OnAttachToPanel(AttachToPanelEvent e)
        {
            if (NodeModel is IObjectReference modelReference && modelReference.ReferencedObject != null)
            {
                TitleLabel?.Bind(new SerializedObject(modelReference.ReferencedObject));
            }
        }

        void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            TitleLabel?.Unbind();
        }

        public override bool IsDroppable()
        {
            var nodeParent = parent as GraphElement;
            var nodeParentSelected = nodeParent?.IsSelected(GraphView) ?? false;
            return base.IsDroppable() && !nodeParentSelected;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (IsInStack && m_SelectedIndex == -1)
                m_SelectedIndex = FindIndexInStack();
        }

        public override bool IsSelected(VisualElement selectionContainer)
        {
            return GraphView.selection.Contains(this);
        }

        public IGraphElementModel GraphElementModel => NodeModel;

        public bool Highlighted
        {
            get => ClassListContains("highlighted");
            set => EnableInClassList("highlighted", value);
        }

        public bool ShouldHighlightItemUsage(IGraphElementModel graphElementModel)
        {
            return false;
        }

        public Func<Node, Store, Vector2, SearcherFilter, bool> CustomSearcherHandler { get; set; }

        public bool HandleCustomSearcher(Vector2 mousePosition, SearcherFilter filter = null)
        {
            if (CustomSearcherHandler != null)
                return CustomSearcherHandler(this, Store, mousePosition, filter);

            // TODO: Refactor this and use interface to manage nodeModel->searcher mapping
            if (NodeModel.ParentStackModel == null || GraphElementModel is PropertyGroupBaseNodeModel)
            {
                return false;
            }

            SearcherService.ShowStackNodes(Store.GetState(), NodeModel.ParentStackModel, mousePosition, item =>
            {
                Store.Dispatch(new ChangeStackedNodeAction(NodeModel, NodeModel.ParentStackModel, item));
            }, new SearcherAdapter($"Change this {NodeModel.Title}"));

            return true;
        }
    }
}