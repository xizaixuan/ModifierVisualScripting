using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class CollapseButton : VisualElement
    {
        bool m_Collapsed;

        static readonly string k_UssClassName = "ge-collapse-button";

        public CollapseButton()
        {
            m_Collapsed = false;

            this.AddStylesheet("CollapseButton.uss");
            AddToClassList(k_UssClassName);

            var icon = new VisualElement { name = "icon" };
            icon.AddToClassList(k_UssClassName + "__icon");
            Add(icon);

            var clickable = new Clickable(SendCollapseEvent);
            this.AddManipulator(clickable);
        }

        void SendCollapseEvent()
        {
            m_Collapsed = !m_Collapsed;
            EnableInClassList(k_UssClassName + "--collapsed", m_Collapsed);

            using (var e = ChangeEvent<bool>.GetPooled(!m_Collapsed, m_Collapsed))
            {
                e.target = this;
                SendEvent(e);
            }
        }
    }
}
