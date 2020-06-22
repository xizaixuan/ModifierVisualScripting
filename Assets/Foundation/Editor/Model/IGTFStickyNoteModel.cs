using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEngine;

namespace Unity.GraphToolsFoundation.Model
{
    public interface IGTFStickyNoteModel : IGTFGraphElementModel, ISelectable, IPositioned, IDeletable, ICopiable
    {
        string Title { get; set; }
        string Contents { get; set; }
        Rect PositionAndSize { get; set; }
        string Theme { get; set; }
        string TextSize { get; set; }
    }
}
