using Unity.Modifier.GraphElements;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    public sealed class SearcherGraphView : GraphView
    {
        public Store Store { get; }

        public SearcherGraphView(Store store)
        {
            Store = store;

            contentContainer.style.flexBasis = StyleKeyword.Auto;

            AddToClassList("searcherGraphView");
            this.AddStylesheet("SearcherGraphView.uss");
        }
    }
}