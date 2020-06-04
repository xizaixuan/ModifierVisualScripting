using System;
using System.Collections.Generic;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class Group : Scope, ICollectibleElement
    {
        private Label m_TitleItem;
        private TextField m_TitleEditor;
        private GroupDropArea m_DropArea;
        private bool m_EditTitleCancelled = false;

        public override string title
        {
            get { return m_TitleItem.text; }
            set
            {
                if (m_TitleItem.text == value)
                    return;

                m_TitleItem.text = value;

                GraphView gv = GetFirstAncestorOfType<GraphView>();

                if (gv != null && gv.groupTitleChanged != null)
                {
                    gv.groupTitleChanged(this, value);
                }

                ScheduleUpdateGeometryFromContent();
            }
        }

        public Group()
        {
            this.AddStylesheet("Group.uss");

            m_DropArea = new GroupDropArea();
            m_DropArea.ClearClassList();
            m_DropArea.name = "dropArea";

            var visualTree = GraphElementsHelper.LoadUXML("GroupTitle.uxml");
            VisualElement titleContainer = visualTree.Instantiate();

            titleContainer.name = "titleContainer";

            m_TitleItem = titleContainer.Q<Label>(name: "titleLabel");

            m_TitleEditor = titleContainer.Q(name: "titleField") as TextField;
            m_TitleEditor.style.display = DisplayStyle.None;

            var titleinput = m_TitleEditor.Q(TextField.textInputUssName);
            titleinput.RegisterCallback<FocusOutEvent>(e => { OnEditTitleFinished(); });
            titleinput.RegisterCallback<KeyDownEvent>(TitleEditorOnKeyDown);

            VisualElement contentContainerPlaceholder = this.Q(name: "contentContainerPlaceholder");
            contentContainerPlaceholder.Insert(0, m_DropArea);

            headerContainer.Add(titleContainer);

            AddToClassList("group");

            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Copiable;

            RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        }

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (element is Group)
            {
                reasonWhyNotAccepted = "Nested group is not supported yet.";
                return false;
            }
            else if (element is Scope)
            {
                reasonWhyNotAccepted = "Nested scope is not supported yet.";
                return false;
            }

            return true;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            GraphView gv = GetFirstAncestorOfType<GraphView>();

            if (gv != null && gv.elementsAddedToGroup != null)
            {
                gv.elementsRemovedFromGroup(this, elements);
            }
        }

        private void TitleEditorOnKeyDown(KeyDownEvent e)
        {
            switch (e.keyCode)
            {
                case KeyCode.Escape:
                    m_EditTitleCancelled = true;
                    m_TitleEditor.Q(TextField.textInputUssName).Blur();
                    break;
                case KeyCode.Return:
                    m_TitleEditor.Q(TextField.textInputUssName).Blur();
                    break;
                default:
                    break;
            }
        }

        private void OnEditTitleFinished()
        {
            m_TitleItem.style.visibility = StyleKeyword.Null;
            m_TitleEditor.style.display = DisplayStyle.None;

            if (!m_EditTitleCancelled)
            {
                string oldName = title;
                title = m_TitleEditor.text;
                OnGroupRenamed(oldName, title);
            }

            m_EditTitleCancelled = false;
        }

        private void OnMouseDownEvent(MouseDownEvent e)
        {
            if (e.clickCount == 2)
            {
                if (HitTest(e.localMousePosition))
                {
                    FocusTitleTextField();
                }
            }
        }

        public void FocusTitleTextField()
        {
            m_TitleEditor.SetValueWithoutNotify(title);
            m_TitleEditor.style.display = DisplayStyle.Flex;
            m_TitleItem.style.visibility = Visibility.Hidden;
            m_TitleEditor.SelectAll();
            m_TitleEditor.Q(TextField.textInputUssName).Focus();
        }

        protected virtual void OnGroupRenamed(string oldName, string newName)
        {

        }

        internal void OnStartDragging(IMouseEvent evt, IEnumerable<GraphElement> elements)
        {
            m_DropArea.OnStartDragging(evt, elements);
        }

        public void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFuc)
        {
            GraphView.CollectElements(containedElements, collectedElementSet, conditionFuc);
        }
    }
}