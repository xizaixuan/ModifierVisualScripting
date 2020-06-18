﻿using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class BlackboardField : GraphElement
    {
        private VisualElement m_ContentItem;
        private Pill m_Pill;
        private TextField m_TextField;
        private Label m_TypeLabel;

        public string text
        {
            get { return m_Pill.text; }
            set { m_Pill.text = value; }
        }

        public string typeText
        {
            get { return m_TypeLabel.text; }
            set { m_TypeLabel.text = value; }
        }

        public Texture icon
        {
            get { return m_Pill.icon; }
            set { m_Pill.icon = value; }
        }

        public bool highlighted
        {
            get { return m_Pill.highlighted; }
            set { m_Pill.highlighted = value; }
        }

        Blackboard m_Blackboard;
        public Blackboard blackboard
        {
            get { return m_Blackboard ?? (m_Blackboard = GetFirstAncestorOfType<Blackboard>()); }
        }

        public BlackboardField() : this(null, "", "") { }
        public BlackboardField(Texture icon, string text, string typeText)
        {
            var tpl = GraphElementsHelper.LoadUXML("BlackboardField.uxml");
            VisualElement mainContainer = tpl.Instantiate();
            this.AddStylesheet(Blackboard.StyleSheetPath);
            mainContainer.AddToClassList("mainContainer");
            mainContainer.pickingMode = PickingMode.Ignore;

            m_ContentItem = mainContainer.Q("contentItem");
            Assert.IsTrue(m_ContentItem != null);

            m_Pill = mainContainer.Q<Pill>("pill");
            Assert.IsTrue(m_Pill != null);

            m_TypeLabel = mainContainer.Q<Label>("typeLabel");
            Assert.IsTrue(m_TypeLabel != null);

            m_TextField = mainContainer.Q<TextField>("textField");
            Assert.IsTrue(m_TextField != null);

            m_TextField.style.display = DisplayStyle.None;

            var textinput = m_TextField.Q(TextField.textInputUssName);
            textinput.RegisterCallback<FocusOutEvent>(e => { OnEditTextFinished(); });

            Add(mainContainer);

            RegisterCallback<MouseDownEvent>(OnMouseDownEvent);

            capabilities |= Capabilities.Selectable | Capabilities.Droppable | Capabilities.Deletable | Capabilities.Renamable;

            ClearClassList();
            AddToClassList("blackboardField");

            this.text = text;
            this.icon = icon;
            this.typeText = typeText;

            this.AddManipulator(new SelectionDropper());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);

            if (evt.eventTypeId == AttachToPanelEvent.TypeId())
            {
                var graphView = blackboard?.graphView;
                if (graphView != null)
                    graphView.RestorePersitentSelectionForElement(this);
            }
        }

        private void OnEditTextFinished()
        {
            m_ContentItem.style.visibility = StyleKeyword.Null;
            m_TextField.style.display = DisplayStyle.None;

            if (text != m_TextField.text)
            {
                if (blackboard?.editTextRequested != null)
                {
                    blackboard.editTextRequested(blackboard, this, m_TextField.text);
                }
                else
                {
                    text = m_TextField.text;
                }
            }
        }

        private void OnMouseDownEvent(MouseDownEvent e)
        {
            if ((e.clickCount == 2) && e.button == (int)MouseButton.LeftMouse && IsRenamable())
            {
                OpenTextEditor();
                e.PreventDefault();
            }
        }

        public void OpenTextEditor()
        {
            m_TextField.SetValueWithoutNotify(text);
            m_TextField.style.display = DisplayStyle.Flex;
            m_ContentItem.style.visibility = Visibility.Hidden;
            m_TextField.Q(TextField.textInputUssName).Focus();
            m_TextField.SelectAll();
        }

        void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
        }
    }
}