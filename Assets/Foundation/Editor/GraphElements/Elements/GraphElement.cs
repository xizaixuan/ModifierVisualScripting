﻿using System;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public abstract class GraphElement : VisualElementBridge, ISelectable
    {
        public Color elementTypeColor { get; set; }

        int m_Layer;
        bool m_LayerIsInline;
        public int layer
        {
            get { return m_Layer; }
            set
            {
                m_LayerIsInline = true;
                if (m_Layer == value)
                    return;
                m_Layer = value;
            }
        }

        public virtual string title
        {
            get { return name; }
            set { throw new NotImplementedException(); }
        }

        public void ResetLayer()
        {
            int prevLayer = m_Layer;
            m_Layer = 0;
            m_LayerIsInline = false;
            customStyle.TryGetValue(s_LayerProperty, out m_Layer);
            UpdateLayer(prevLayer);
        }

        static CustomStyleProperty<int> s_LayerProperty = new CustomStyleProperty<int>("--layer");

        private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            OnCustomStyleResolved(e.customStyle);
        }

        protected virtual void OnCustomStyleResolved(ICustomStyle style)
        {
            int prevLayer = m_Layer;
            if (!m_LayerIsInline)
                style.TryGetValue(s_LayerProperty, out m_Layer);

            UpdateLayer(prevLayer);
        }

        private void UpdateLayer(int prevLayer)
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

        private Capabilities m_Capabilities;

        public Capabilities capabilities
        {
            get { return m_Capabilities; }
            set
            {
                if (m_Capabilities == value)
                    return;

                m_Capabilities = value;

                if (IsSelectable() && m_ClickSelector == null)
                {
                    m_ClickSelector = new ClickSelector();
                    this.AddManipulator(m_ClickSelector);
                }
                else if (!IsSelectable() && m_ClickSelector != null)
                {
                    this.RemoveManipulator(m_ClickSelector);
                    m_ClickSelector = null;
                }
            }
        }

        internal ResizeRestriction resizeRestriction { get; set; }

        private bool m_Selected;

        public bool selected
        {
            get { return m_Selected; }
            set
            {
                if ((capabilities & Capabilities.Selectable) != Capabilities.Selectable)
                    return;

                if (m_Selected == value)
                    return;

                m_Selected = value;

                if (m_Selected)
                {
                    this.SetCheckedPseudoState(true);
                }
                else
                {
                    this.SetCheckedPseudoState(false);
                }
            }
        }

        protected GraphElement()
        {
            ClearClassList();
            AddToClassList("graphElement");
            elementTypeColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);

            viewDataKey = Guid.NewGuid().ToString();

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        ClickSelector m_ClickSelector;

        public virtual bool IsSelectable()
        {
            return (capabilities & Capabilities.Selectable) == Capabilities.Selectable && visible && resolvedStyle.display != DisplayStyle.None;
        }

        public virtual bool IsMovable()
        {
            return (capabilities & Capabilities.Movable) == Capabilities.Movable;
        }

        public virtual bool IsResizable()
        {
            return (capabilities & Capabilities.Resizable) == Capabilities.Resizable;
        }

        public virtual bool IsDroppable()
        {
            return (capabilities & Capabilities.Droppable) == Capabilities.Droppable;
        }

        public virtual bool IsAscendable()
        {
            return (capabilities & Capabilities.Ascendable) == Capabilities.Ascendable;
        }

        public virtual bool IsRenamable()
        {
            return (capabilities & Capabilities.Renamable) == Capabilities.Renamable;
        }

        public virtual bool IsCopiable()
        {
            return (capabilities & Capabilities.Copiable) == Capabilities.Copiable;
        }

        static Vector2 MultiplyMatrix44Point2(Matrix4x4 lhs, Vector2 point)
        {
            Vector2 res;
            res.x = lhs.m00 * point.x + lhs.m01 * point.y + lhs.m03;
            res.y = lhs.m10 * point.x + lhs.m11 * point.y + lhs.m13;
            return res;
        }

        public virtual Vector3 GetGlobalCenter()
        {
            var globalCenter = layout.center + parent.layout.position;
            return MultiplyMatrix44Point2(parent.worldTransform, globalCenter);
        }

        // TODO: Temporary transition function.
        public virtual void UpdatePresenterPosition()
        {
            // This can be overridden by derived class to get notified when a manipulator
            // has *finished* changing the layout (size or position) of this element.
        }

        public virtual Rect GetPosition()
        {
            return layout;
        }

        public virtual void SetPosition(Rect newPos)
        {
            this.SetLayout(newPos);
        }

        public virtual void OnSelected()
        {
            if (IsAscendable() && resolvedStyle.position != Position.Relative)
                BringToFront();
        }

        public virtual void OnUnselected()
        {
        }

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

        public virtual void Unselect(VisualElement selectionCountainer)
        {
            var selection = selectionCountainer as ISelection;
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