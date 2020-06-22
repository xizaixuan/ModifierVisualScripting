using System;
using System.Collections.Generic;
using Unity.GraphToolsFoundation.Model;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public enum StickyNoteTheme
    {
        Classic,
        Black,
        Dark,
        Orange,
        Green,
        Blue,
        Red,
        Purple,
        Teal
    }

    public enum StickyNoteFontSize
    {
        Small,
        Medium,
        Large,
        Huge
    }

    public class StickyNote : GraphElement, IResizable, IMovable
    {
        public new class UxmlFactory : UxmlFactory<StickyNote> { }

        public IGTFStickyNoteModel StickyNoteModel => Model as IGTFStickyNoteModel;

        public static readonly Vector2 defaultSize = new Vector2(200, 160);

        Label m_Title;
        TextField m_TitleField;
        Label m_Content;
        TextField m_ContentField;

        public StickyNote()
        {
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        protected override void BuildUI()
        {
            base.BuildUI();

            GraphElementsHelper.LoadTemplateAndStylesheet(this, "StickyNote", "ge-sticky-note");
            this.AddStylesheet("Selectable.uss");

            usageHints = UsageHints.DynamicTransform;

            m_Title = this.MandatoryQ<Label>(name: "title");
            m_TitleField = this.MandatoryQ<TextField>(name: "title-field");
            m_Content = this.MandatoryQ<Label>(name: "contents");
            m_ContentField = this.MandatoryQ<TextField>(name: "contents-field");

            m_Title?.RegisterCallback<MouseDownEvent>(OnTitleMouseDown);

            m_TitleField.style.display = DisplayStyle.None;
            m_TitleField.Q("unity-text-input").RegisterCallback<BlurEvent>(OnTitleBlur);
            m_TitleField.multiline = true;
            m_TitleField.isDelayed = true;
            m_TitleField.RegisterCallback<ChangeEvent<string>>(OnContentChange);

            m_Content.RegisterCallback<MouseDownEvent>(OnContentsMouseDown);

            m_ContentField.style.display = DisplayStyle.None;
            m_ContentField.multiline = true;
            m_ContentField.isDelayed = true;
            m_ContentField.Q("unity-text-input").RegisterCallback<BlurEvent>(OnContentsBlur);
            m_ContentField.RegisterCallback<ChangeEvent<string>>(OnContentChange);

            AddToClassList("selectable");
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            var newPos = StickyNoteModel.PositionAndSize;
            style.left = newPos.x;
            style.top = newPos.y;
            style.width = newPos.width;
            style.height = newPos.height;

            m_Title.text = StickyNoteModel.Title;
            m_TitleField.value = StickyNoteModel.Title;
            m_Content.text = StickyNoteModel.Contents;
            m_ContentField.value = StickyNoteModel.Contents;

            UpdateThemeClasses();
            UpdateSizeClasses();

            if (m_Title != null)
            {
                if (!string.IsNullOrEmpty(m_Title.text))
                {
                    m_Title.RemoveFromClassList("empty");
                }
                else
                {
                    m_Title.AddToClassList("empty");
                }
            }
        }

        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        public static IEnumerable<string> GetThemes()
        {
            foreach (var s in Enum.GetNames(typeof(StickyNoteTheme)))
            {
                yield return s;
            }
        }

        public static IEnumerable<string> GetSizes()
        {
            foreach (var s in Enum.GetNames(typeof(StickyNoteFontSize)))
            {
                yield return s;
            }
        }

        void OnFitToText(DropdownMenuAction a)
        {
            FitText(false);
        }

        void FitText(bool onlyIfSmaller)
        {
            Vector2 preferredTitleSize = Vector2.zero;
            if (!string.IsNullOrEmpty(m_Title.text))
                preferredTitleSize = m_Title.MeasureTextSize(m_Title.text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined); // This is the size of the string with the current title font and such

            preferredTitleSize += AllExtraSpace(m_Title);
            preferredTitleSize.x += m_Title.ChangeCoordinatesTo(this, Vector2.zero).x + resolvedStyle.width - m_Title.ChangeCoordinatesTo(this, new Vector2(m_Title.layout.width, 0)).x;

            Vector2 preferredContentsSizeOneLine = m_Content.MeasureTextSize(m_Content.text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);

            Vector2 contentExtraSpace = AllExtraSpace(m_Content);
            preferredContentsSizeOneLine += contentExtraSpace;

            Vector2 extraSpace = new Vector2(resolvedStyle.width, resolvedStyle.height) - m_Content.ChangeCoordinatesTo(this, new Vector2(m_Content.layout.width, m_Content.layout.height));
            extraSpace += m_Title.ChangeCoordinatesTo(this, Vector2.zero);
            preferredContentsSizeOneLine += extraSpace;

            float width = 0;
            float height = 0;
            // The content in one line is smaller than the current width.
            // Set the width to fit both title and content.
            // Set the height to have only one line in the content
            if (preferredContentsSizeOneLine.x < Mathf.Max(preferredTitleSize.x, resolvedStyle.width))
            {
                width = Mathf.Max(preferredContentsSizeOneLine.x, preferredTitleSize.x);
                height = preferredContentsSizeOneLine.y + preferredTitleSize.y;
            }
            else // The width is not enough for the content: keep the width or use the title width if bigger.
            {
                width = Mathf.Max(preferredTitleSize.x + extraSpace.x, resolvedStyle.width);
                float contextWidth = width - extraSpace.x - contentExtraSpace.x;
                Vector2 preferredContentsSize = m_Content.MeasureTextSize(m_Content.text, contextWidth, MeasureMode.Exactly, 0, MeasureMode.Undefined);

                preferredContentsSize += contentExtraSpace;

                height = preferredTitleSize.y + preferredContentsSize.y + extraSpace.y;
            }

            ResizeFlags resizeWhat = ResizeFlags.None;
            if (!onlyIfSmaller || resolvedStyle.width < width)
            {
                resizeWhat |= ResizeFlags.Width;
                style.width = width;
            }

            if (!onlyIfSmaller || resolvedStyle.height < height)
            {
                resizeWhat |= ResizeFlags.Height;
                style.height = height;
            }

            if (this is IResizable && resizeWhat != ResizeFlags.None)
            {
                Rect newRect = new Rect(0, 0, width, height);
                (this as IResizable).OnResized(newRect, resizeWhat);
            }
        }

        static readonly string k_ThemeClassNamePrefix = "ge-sticky-note--theme-";
        static readonly string k_SizeClassNamePrefix = "ge-sticky-note--size-";

        void UpdateThemeClasses()
        {
            this.PrefixRemoveFromClassList(k_ThemeClassNamePrefix);
            AddToClassList(k_ThemeClassNamePrefix + StickyNoteModel.Theme.ToKebabCase());
        }

        void UpdateSizeClasses()
        {
            this.PrefixRemoveFromClassList(k_SizeClassNamePrefix);
            AddToClassList(k_SizeClassNamePrefix + StickyNoteModel.TextSize.ToKebabCase());
        }

        void OnContentChange(ChangeEvent<string> e)
        {
            Store.Dispatch(new UpdateStickyNoteAction(StickyNoteModel, m_TitleField.value, m_ContentField.value));
        }

        public virtual void OnResized(Rect newRect, ResizeFlags resizeWhat)
        {
            if (resizeWhat != ResizeFlags.None)
            {
                Store.Dispatch(new ResizeStickyNoteAction(StickyNoteModel, newRect, resizeWhat));
            }
        }

        void OnTitleMouseDown(MouseDownEvent e)
        {
            if (e.button == (int)MouseButton.LeftMouse && e.clickCount == 2)
            {
                m_TitleField.RemoveFromClassList("empty");
                m_Title.style.display = DisplayStyle.None;
                m_TitleField.style.display = StyleKeyword.Null;

                m_TitleField.Q(TextField.textInputUssName).Focus();
                m_TitleField.SelectAll();

                e.StopPropagation();
                e.PreventDefault();
            }
        }

        void OnTitleBlur(BlurEvent e)
        {
            m_Title.style.display = StyleKeyword.Null;
            m_TitleField.style.display = DisplayStyle.None;
        }

        void OnContentsMouseDown(MouseDownEvent e)
        {
            if (e.button == (int)MouseButton.LeftMouse && e.clickCount == 2)
            {
                m_Content.style.display = DisplayStyle.None;
                m_ContentField.style.display = StyleKeyword.Null;
                m_ContentField.Q(TextField.textInputUssName).Focus();
                m_ContentField.SelectAll();
                e.StopPropagation();
                e.PreventDefault();
            }
        }

        void OnContentsBlur(BlurEvent e)
        {
            m_Content.style.display = StyleKeyword.Null;
            m_ContentField.style.display = DisplayStyle.None;
        }

        static Vector2 AllExtraSpace(VisualElement element)
        {
            return new Vector2(
                element.resolvedStyle.marginLeft + element.resolvedStyle.marginRight + element.resolvedStyle.paddingLeft + element.resolvedStyle.paddingRight + element.resolvedStyle.borderRightWidth + element.resolvedStyle.borderLeftWidth,
                element.resolvedStyle.marginTop + element.resolvedStyle.marginBottom + element.resolvedStyle.paddingTop + element.resolvedStyle.paddingBottom + element.resolvedStyle.borderBottomWidth + element.resolvedStyle.borderTopWidth
            );
        }

        public void UpdatePinning()
        {
        }

        public bool IsMovable => true;
    }
}