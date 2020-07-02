using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class SmartNodeTitle : VisualElement
    {
        VisualElement m_Label;
        Image m_Icon;

        static readonly string k_UssClassName = "ge-smart-title";

        public SmartNodeTitle(bool isEditable, Texture icon)
        {
            AddToClassList(k_UssClassName);

            if (icon)
            {
                m_Icon = new Image();
                m_Icon.AddToClassList(k_UssClassName + "__icon");
                m_Icon.image = icon;
                Add(m_Icon);
            }

            if (isEditable)
            {
                AddToClassList(k_UssClassName + "--editable");
                m_Label = new EditableLabel { name = "smart-title-label" };
            }
            else
            {
                m_Label = new Label { name = "smart-title-label" };
            }

            m_Label.AddToClassList(k_UssClassName + "__label");
            Add(m_Label);
        }

        public bool IsEditable => m_Label is EditableLabel;

        public string Text
        {
            set
            {
                if (m_Label is EditableLabel editableLabel)
                    editableLabel.SetValueWithoutNotify(value);
                else if (m_Label is Label label)
                    label.text = value;
            }
        }

        public string BindingPath
        {
            set
            {
                if (m_Label is EditableLabel editableLabel)
                    editableLabel.bindingPath = value;
                else if (m_Label is Label label)
                    label.bindingPath = value;
            }
        }
    }
}