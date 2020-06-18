using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IPlacematModel : IGraphElementModel, IUndoRedoAware
    {
        string Title { get; }
        Rect Position { get; }
        Color Color { get; }
        bool Collapsed { get; }
        int ZOrder { get; }
        List<string> HiddenElementsGuid { get; }
        bool Destroyed { get; }
    }
}