
using System;
using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.Editor.Plugins;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [Serializable]
    public struct OpenedGraph
    {
        public GraphAssetModel GraphAssetModel;

        public OpenedGraph(GraphAssetModel graphAssetModel)
        {
            GraphAssetModel = graphAssetModel;
        }
    }

    public interface IEditorDataModel
    {
        UpdateFlags UpdateFlags { get; }
        IGraphElementModel ElementModelToRename { get; set; }
        GUID NodeToFrameGuid { get; set; }
        int CurrentGraphIndex { get; }
        VSPreferences Preferences { get; }
        IPluginRepository PluginRepository { get; }
        List<OpenedGraph> PreviousGraphModels { get; }
        int UpdateCounter { get; set; }
        bool TracingEnabled { get; set; }

        void SetUpdateFlag(UpdateFlags flag);

        void RequestCompilation(RequestCompilationOptions options);

        bool ShouldSelectElementUponCreation(IHasGraphElementModel hasGraphElementModel);

        void SelectElementsUponCreation(IEnumerable<IGraphElementModel> graphElementModels, bool select);

        void ClearElementsToSelectUponCreation();

        bool ShouldExpandBlackboardRowUponCreation(string rowName);

        void ExpandBlackboardRowsUponCreation(IEnumerable<string> rowNames, bool expand);

        bool ShouldExpandElementUponCreation(IVisualScriptingField visualScriptingField);

        void ExpandElementsUponCreation(IEnumerable<IVisualScriptingField> visualScriptingFields, bool expand);
    }

    public enum RequestCompilationOptions
    {
        Default,
        SaveGraph,
    }
}