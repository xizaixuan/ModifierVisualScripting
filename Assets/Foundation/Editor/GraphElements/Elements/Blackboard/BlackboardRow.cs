﻿using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class BlackboardRow : GraphElement
    {
        private VisualElement m_Root;
        private Button m_ExpandButton;
        private VisualElement m_ItemContainer;
        private VisualElement m_PropertyViewContainer;
        private bool m_Expanded = true;

        public bool expanded
        {
            get { return m_Expanded; }
            set
            {
                if (m_Expanded == value)
                {
                    return;
                }

                m_Expanded = value;

                if (m_Expanded)
                {
                    m_Root.Add(m_PropertyViewContainer);
                    AddToClassList("expanded");
                }
                else
                {
                    m_Root.Remove(m_PropertyViewContainer);
                    RemoveFromClassList("expanded");
                }
            }
        }

        public BlackboardRow(VisualElement item, VisualElement propertyView)
        {
            var tpl = GraphElementsHelper.LoadUXML("BlackboardRow.uxml");
            this.AddStylesheet(Blackboard.StyleSheetPath);

            VisualElement mainContainer = tpl.Instantiate();

            mainContainer.AddToClassList("mainContainer");

            m_Root = mainContainer.Q("root");
            m_ItemContainer = mainContainer.Q("itemContainer");
            m_PropertyViewContainer = mainContainer.Q("propertyViewContainer");

            m_ExpandButton = mainContainer.Q<Button>("expandButton");
            m_ExpandButton.clickable.clicked += () => expanded = !expanded;

            Add(mainContainer);

            ClearClassList();
            AddToClassList("blackboardRow");

            m_ItemContainer.Add(item);
            m_PropertyViewContainer.Add(propertyView);

            expanded = false;
        }
    }
}