using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    class StickyNote : Unity.Modifier.GraphElements.StickyNote, IHasGraphElementModel, IMovable
    {
        readonly Store m_Store;
        readonly IStickyNoteModel stickyNoteModel;
        public IGraphElementModel GraphElementModel => stickyNoteModel;

        public bool NeedStoreDispatch => false;

        // ReSharper disable once UnusedParameter.Local
        public StickyNote(Store store, IStickyNoteModel model, Rect position, GraphView graphView)
            : base(position.position)
        {
            m_Store = store;
            stickyNoteModel = model;

            theme = ConvertTheme(model.Theme);
            UpdateThemeClasses();

            fontSize = (StickyNoteFontSize)model.TextSize;

            title = model.Title;
            contents = model.Contents;
            base.SetPosition(position);

            RegisterCallback<StickyNoteChangeEvent>(OnChange);
        }

        StickyNoteTheme ConvertTheme(StickyNoteColorTheme modelTheme)
        {
            switch (modelTheme)
            {
                case StickyNoteColorTheme.Dark:
                    return StickyNoteTheme.Black;
                default:
                    return StickyNoteTheme.Classic;
            }
        }

        void UpdateThemeClasses()
        {
            foreach (StickyNoteColorTheme value in System.Enum.GetValues(typeof(StickyNoteColorTheme)))
            {
                if (stickyNoteModel.Theme != value)
                    RemoveFromClassList("theme-" + value.ToString().ToLower());
                else
                    AddToClassList("theme-" + value.ToString().ToLower());
            }
        }

        public override void OnResized()
        {
            var topLeftOffset = new Vector2(resolvedStyle.marginLeft + resolvedStyle.paddingLeft + resolvedStyle.borderLeftWidth,
                resolvedStyle.marginTop + resolvedStyle.paddingTop + resolvedStyle.borderTopWidth);
            m_Store.Dispatch(new ResizeStickyNoteAction(stickyNoteModel,
                new Rect(layout.position - topLeftOffset, layout.size)));
        }

        void OnChange(StickyNoteChangeEvent evt)
        {
            switch (evt.change)
            {
                case StickyNoteChange.Title:
                case StickyNoteChange.Contents:
                    m_Store.Dispatch(new UpdateStickyNoteAction(stickyNoteModel, title, contents));
                    break;
            }
        }

        public void UpdatePinning()
        {
        }
    }
}