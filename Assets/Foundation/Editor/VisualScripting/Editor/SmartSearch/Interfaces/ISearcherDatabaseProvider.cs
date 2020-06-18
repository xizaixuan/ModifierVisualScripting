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
        List<SearcherDatabase> GetReferenceItemsSearcherDatabases();
        List<SearcherDatabase> GetTypesSearcherDatabases();
        List<SearcherDatabase> GetTypeMembersSearcherDatabases(TypeHandle typeHandle);
        List<SearcherDatabase> GetGraphVariablesSearcherDatabases(IGraphModel graphModel, IFunctionModel functionModel = null);
        List<SearcherDatabase> GetDynamicSearcherDatabases(IPortModel portModel);
        void ClearGraphElementsSearcherDatabases();
        void ClearReferenceItemsSearcherDatabases();
        void ClearTypesItemsSearcherDatabases();
        void ClearTypeMembersSearcherDatabases();
        void ClearGraphVariablesSearcherDatabases();
    }
}