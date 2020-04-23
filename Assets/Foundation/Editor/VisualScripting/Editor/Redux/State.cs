
using System;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class State : IDisposable
    {
        public IGraphAssetModel AssetModel { get; set; }

        public IGraphModel CurrentGraphModel => AssetModel?.GraphModel;

        public IEditorDataModel EditorDataModel { get; private set; }

        public ICompilationResultModel CompilationResultModel { get; private set; }

        public int CurrentTracingStep;

        public State(IEditorDataModel editorDataModel)
        {
            CompilationResultModel = new CompilationResultModel();
            EditorDataModel = editorDataModel;
            CurrentTracingStep = -1;
        }

        public void Dispose()
        {
            UnloadCurrentGraphAsset();
            CompilationResultModel = null;
            EditorDataModel = null;
        }

        public void UnloadCurrentGraphAsset()
        {
            AssetModel?.Dispose();
            AssetModel = null;
        }
    }
}