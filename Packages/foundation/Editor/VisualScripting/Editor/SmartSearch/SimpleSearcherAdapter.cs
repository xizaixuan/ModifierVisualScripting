using UnityEditor.Searcher;
namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    public class SimpleSearcherAdapter : SearcherAdapter
    {
        public SimpleSearcherAdapter(string title)
            : base(title) { }

        // TODO: Disable details panel for now
        public override bool HasDetailsPanel => false;
    }
}