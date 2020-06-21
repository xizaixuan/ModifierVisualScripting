using JetBrains.Annotations;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    [PublicAPI]
    public enum StickyNoteTextSize
    {
        Small,
        Medium,
        Large,
        Huge
    }

    [PublicAPI]
    public enum StickyNoteColorTheme
    {
        Classic,
        Dark,
        Orange,
        Green,
        Blue,
        Red,
        Purple,
        Teal
    }

    public interface IStickyNoteModel : IGraphElementModel
    {
        bool Destroyed { get; }
    }
}