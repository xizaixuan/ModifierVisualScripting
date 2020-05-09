using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public abstract class GraphElement : VisualElementBridge, ISelectable
    {
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

        ClickSelector m_ClickSelector;

        public virtual bool IsSelectable()
        {
            return (capabilities & Capabilities.Selectable) == Capabilities.Selectable && visible && resolvedStyle.display != DisplayStyle.None;
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
                    selection.AddToSelectioin(this);
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