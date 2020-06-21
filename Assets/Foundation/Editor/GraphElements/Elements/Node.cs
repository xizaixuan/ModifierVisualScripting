using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class Node : GraphElement, IMovable
    {
        public IGTFNodeModel NodeModel => Model as IGTFNodeModel;
        [CanBeNull]
        protected VisualElement TitleContainer { get; set; }
        [CanBeNull]
        protected SmartNodeTitle TitleLabel { get; set; }

        protected ContextualMenuManipulator m_ContextualMenuManipulator;

        public static readonly string k_UssClassName = "ge-node";

        public Node()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            m_ContextualMenuManipulator = new ContextualMenuManipulator(BuildContextualMenu);
            this.AddManipulator(m_ContextualMenuManipulator);
        }

        protected override void BuildUI()
        {
            base.BuildUI();

            usageHints = UsageHints.DynamicTransform;

            AddToClassList(k_UssClassName);
            this.AddStylesheet("Node.uss");

            if (NodeModel is IHasTitle)
            {
                bool isRenamable = Model is IRenamable renamable && renamable.IsRenamable;
                TitleContainer = new VisualElement { name = "title-container" };
                TitleContainer.AddToClassList(k_UssClassName + "__title-container");
                TitleLabel = new SmartNodeTitle(isRenamable, null);
                TitleLabel.AddToClassList(k_UssClassName + "__title");

                TitleContainer.Add(TitleLabel);
                Add(TitleContainer);

                if (isRenamable)
                    TitleLabel.RegisterCallback<ChangeEvent<string>>(OnRename);
            }
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            var newPos = NodeModel.Position;
            style.left = newPos.x;
            style.top = newPos.y;

            if (TitleLabel != null)
            {
                TitleLabel.Text = (NodeModel as IHasTitle)?.Title.Nicify() ?? String.Empty;
            }

            EnableInClassList(k_UssClassName + "--empty", childCount == 0);
        }

        protected virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        protected virtual void UpdateEdgeLayout() { }

        public virtual void MarkEdgesDirty() { }

        void OnGeometryChanged(GeometryChangedEvent e)
        {
            if (e.target == this)
                UpdateEdgeLayout();
        }

        void OnRename(ChangeEvent<string> e)
        {
            Store.Dispatch(new RenameElementAction(Model as IRenamable, e.newValue));
        }

        public virtual void UpdatePinning()
        {
        }

        public virtual bool IsMovable => true;
    }
}