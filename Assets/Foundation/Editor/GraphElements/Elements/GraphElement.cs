using System;
using Unity.Modifier.GraphToolsFoundation.Model;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public abstract class GraphElement : VisualElementBridge, ISelectable, IGraphElement
    {
        public Color MinimapColor { get; protected set; }

        int m_Layer;
        bool m_LayerIsInline;
        public int layer
        {
            get => m_Layer;
            set
            {
                m_LayerIsInline = true;
                m_Layer = value;
            }
        }

        public virtual bool ShowInMiniMap { get; set; } = true;

        public void ResetLayer()
        {
            int prevLayer = m_Layer;
            m_Layer = 0;
            m_LayerIsInline = false;
            customStyle.TryGetValue(s_LayerProperty, out m_Layer);
            UpdateLayer(prevLayer);
        }

        static CustomStyleProperty<int> s_LayerProperty = new CustomStyleProperty<int>("--layer");

        void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            OnCustomStyleResolved(e.customStyle);
        }

        protected virtual void OnCustomStyleResolved(ICustomStyle resolvedCustomStyle)
        {
            int prevLayer = m_Layer;
            if (!m_LayerIsInline)
                resolvedCustomStyle.TryGetValue(s_LayerProperty, out m_Layer);

            UpdateLayer(prevLayer);
        }

        void UpdateLayer(int prevLayer)
        {
            if (prevLayer != m_Layer)
            {
                GraphView view = GetFirstAncestorOfType<GraphView>();
                if (view != null)
                {
                    view.ChangeLayer(this);
                }
            }
        }

        internal ResizeRestriction resizeRestriction { get; set; }

        bool m_Selected;
        public bool selected
        {
            get => m_Selected;
            set
            {
                // Set new value (toggle old value)
                if (!IsSelectable())
                    return;

                if (m_Selected == value)
                    return;

                m_Selected = value;

                this.SetCheckedPseudoState(m_Selected);
            }
        }

        protected GraphElement()
        {
            AddToClassList("graph-element");
            MinimapColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);

            viewDataKey = Guid.NewGuid().ToString();

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public void Setup(IGTFGraphElementModel model, IStore store, GraphView graphView)
        {
            Model = model;
            Store = store;
            GraphView = graphView;

            BuildUI();
            UpdateFromModel();
        }

        protected virtual void BuildUI()
        {
            // PF: Uncomment when all graph elements have a clean Setup/BuildUI/UpdateFromModel path.
            // BlackboardVariableField do not.
            // FIXME: make it a separate step from BuildUI(), since subclasses may want to do stuff before their base.
            // Clear();
        }

        public virtual void UpdateFromModel()
        {
            if (IsSelectable() && ClickSelector == null)
            {
                ClickSelector = new ClickSelector();
                this.AddManipulator(ClickSelector);
            }
            else if (!IsSelectable() && ClickSelector != null)
            {
                this.RemoveManipulator(ClickSelector);
                ClickSelector = null;
            }
        }

        protected ClickSelector ClickSelector { get; private set; }

        public IGTFGraphElementModel Model { get; private set; }

        // PF make setter private
        protected IStore Store { get; set; }
        public GraphView GraphView { get; private set; }


        public virtual bool IsSelectable()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.ISelectable;
        }

        public virtual bool IsPositioned()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.IPositioned;
        }

        public virtual bool IsDeletable()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.IDeletable;
        }

        public virtual bool IsResizable()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.IResizable;
        }

        public virtual bool IsDroppable()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.IDroppable;
        }

        public virtual bool IsAscendable()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.IAscendable;
        }

        public virtual bool IsRenamable()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.IRenamable;
        }

        public virtual bool IsCopiable()
        {
            return Model is Unity.Modifier.GraphToolsFoundation.Model.ICopiable copiable && copiable.IsCopiable;
        }

        static Vector2 MultiplyMatrix44Point2(Matrix4x4 lhs, Vector2 point)
        {
            Vector2 res;
            res.x = lhs.m00 * point.x + lhs.m01 * point.y + lhs.m03;
            res.y = lhs.m10 * point.x + lhs.m11 * point.y + lhs.m13;
            return res;
        }

        // PF: remove
        public Rect GetPosition()
        {
            return layout;
        }

        public virtual void SetPosition(Rect newPos)
        {
            style.left = newPos.x;
            style.top = newPos.y;
        }

        public virtual void OnSelected()
        {
            if (IsAscendable() && resolvedStyle.position != Position.Relative)
                BringToFront();
        }

        public virtual void OnUnselected()
        {
        }

        // TODO: remove
        public virtual bool HitTest(Vector2 localPoint)
        {
            return ContainsPoint(localPoint);
        }

        public virtual void Select(VisualElement selectionContainer, bool additive)
        {
            var selection = selectionContainer as ISelection;
            if (selection != null)
            {
                if (!selection.selection.Contains(this))
                {
                    if (!additive)
                        selection.ClearSelection();

                    selection.AddToSelection(this);
                }
            }
        }

        public virtual void Unselect(VisualElement selectionContainer)
        {
            var selection = selectionContainer as ISelection;
            if (selection != null)
            {
                if (selection.selection.Contains(this))
                {
                    selection.RemoveFromSelection(this);
                }
            }
        }

        public virtual bool IsSelected(VisualElement selectionContainer)
        {
            var selection = selectionContainer as ISelection;
            if (selection != null)
            {
                if (selection.selection.Contains(this))
                {
                    return true;
                }
            }

            return false;
        }
    }
}