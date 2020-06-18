﻿using Unity.Modifier.GraphElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    class EdgeBubble : Label
    {
        TextField TextField { get; }

        Attacher m_Attacher;

        public override string text
        {
            get => base.text;
            set
            {
                if (base.text == value)
                    return;
                base.text = value;
                (parent as Edge)?.Rename(value);
            }
        }

        public EdgeBubble()
        {
            TextField = new TextField { isDelayed = true };

            AddToClassList("edgeBubble");
        }

        void OnBlur(BlurEvent evt)
        {
            SaveAndClose();
        }

        void SaveAndClose()
        {
            text = TextField.text;
            Close();
        }

        void Close()
        {
            TextField.value = text;
            TextField.RemoveFromHierarchy();
            TextField.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            TextField.UnregisterCallback<BlurEvent>(OnBlur);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                    SaveAndClose();
                    break;
                case KeyCode.Escape:
                    Close();
                    break;
            }
        }

        public void AttachTo(EdgeControl edgeControlTarget, SpriteAlignment align)
        {
            if (m_Attacher?.target == edgeControlTarget && m_Attacher?.alignment == align)
                return;

            Detach();

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_Attacher = new Attacher(this, edgeControlTarget, align);
        }

        public void Detach()
        {
            if (m_Attacher == null)
                return;

            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            m_Attacher.Detach();
            m_Attacher = null;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ComputeTextSize();
        }

        public void SetAttacherOffset(Vector2 offset)
        {
            if (m_Attacher != null)
                m_Attacher.offset = offset;
        }

        void ComputeTextSize()
        {
            if (style.fontSize == 0)
                return;

            var newSize = DoMeasure(resolvedStyle.maxWidth.value, MeasureMode.AtMost, 0, MeasureMode.Undefined);

            style.width = newSize.x +
                resolvedStyle.marginLeft +
                resolvedStyle.marginRight +
                resolvedStyle.borderLeftWidth +
                resolvedStyle.borderRightWidth +
                resolvedStyle.paddingLeft +
                resolvedStyle.paddingRight;

            style.height = newSize.y +
                resolvedStyle.marginTop +
                resolvedStyle.marginBottom +
                resolvedStyle.borderTopWidth +
                resolvedStyle.borderBottomWidth +
                resolvedStyle.paddingTop +
                resolvedStyle.paddingBottom;
        }
    }
}