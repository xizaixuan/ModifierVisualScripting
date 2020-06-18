using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface ISearcherFilter
    {
        SearcherFilter GetFilter(INodeModel model);
    }
}