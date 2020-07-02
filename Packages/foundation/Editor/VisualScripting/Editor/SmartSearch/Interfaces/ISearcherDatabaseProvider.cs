using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Searcher;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    public interface ISearcherDatabaseProvider
    {
        List<SearcherDatabase> GetGraphElementsSearcherDatabases();
        List<SearcherDatabase> GetTypesSearcherDatabases();
        List<SearcherDatabase> GetGraphVariablesSearcherDatabases(IGraphModel graphModel);
        List<SearcherDatabase> GetDynamicSearcherDatabases(IPortModel portModel);
        void ClearGraphElementsSearcherDatabases();
        void ClearTypesItemsSearcherDatabases();
        void ClearTypeMembersSearcherDatabases();
        void ClearGraphVariablesSearcherDatabases();
    }
}