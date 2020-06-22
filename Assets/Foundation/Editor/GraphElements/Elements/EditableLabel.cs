﻿using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class EditableLabel : VisualElementBridge
    {
        public new class UxmlFactory : UxmlFactory<EditableLabel, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlBoolAttributeDescription m_Multiline = new UxmlBoolAttributeDescription { name = "multiline" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                EditableLabel field = ((EditableLabel)ve);
                field.multiline = m_Multiline.GetValueFromBag(bag, cc);
                base.Init(ve, bag, cc);
            }
        }

        Label m_Label;
        TextField m_TextField;

        public EditableLabel()
        {
            SetIsCompositeRoot();
            focusable = true;

            GraphElementsHelper.LoadTemplateAndStylesheet(this, "EditableLabel", "ge-editable-label");

            m_Label = this.Q<Label>(name: "label");
            m_TextField = this.Q<TextField>(name: "text-field");

            m_Label.RegisterCallback<MouseDownEvent>(OnLabelMouseDown);

            m_TextField.style.display = DisplayStyle.None;
            m_TextField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            m_TextField.RegisterCallback<BlurEvent>(OnFieldBlur);
            m_TextField.RegisterCallback<ChangeEvent<string>>(OnChange);
            m_TextField.isDelayed = true;
        }

        public void SetValueWithoutNotify(string value)
        {
            ((INotifyValueChanged<string>)m_Label).SetValueWithoutNotify(value);
            m_TextField.SetValueWithoutNotify(value);
        }

        public bool multiline
        {
            set => m_TextField.multiline = value;
        }

        public string bindingPath
        {
            set => m_Label.bindingPath = value;
        }

        string m_CurrentValue;
        void OnLabelMouseDown(MouseDownEvent e)
        {
            if (e.target == e.currentTarget)
            {
                if (e.button == (int)MouseButton.LeftMouse)
                {
                    if (e.clickCount == 1)
                    {
                        // Prevent focusing on single click.
                        e.PreventDefault();
                    }
                    else if (e.clickCount == 2)
                    {
                        m_CurrentValue = m_Label.text;

                        m_Label.style.display = DisplayStyle.None;
                        m_TextField.style.display = StyleKeyword.Null;

                        m_TextField.Q(TextField.textInputUssName).Focus();
                        m_TextField.SelectAll();

                        e.StopPropagation();
                        e.PreventDefault();
                    }
                }
            }
        }

        void OnKeyDown(KeyDownEvent e)
        {
            if (e.target == e.currentTarget)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    m_TextField.SetValueWithoutNotify(m_CurrentValue);
                    m_TextField.Blur();
                }
            }
        }

        void OnFieldBlur(BlurEvent e)
        {
            if (e.target == e.currentTarget)
                m_Label.style.display = StyleKeyword.Null;
            m_TextField.style.display = DisplayStyle.None;
        }

        void OnChange(ChangeEvent<string> e)
        {
            if (e.target == e.currentTarget)
                ((INotifyValueChanged<string>)m_Label).SetValueWithoutNotify(m_TextField.value);
        }
    }
}