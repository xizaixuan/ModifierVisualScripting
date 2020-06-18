using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface ICustomSearcherHandler
    {
        bool HandleCustomSearcher(Vector2 mousePosition, SearcherFilter filter = null);
    }
}