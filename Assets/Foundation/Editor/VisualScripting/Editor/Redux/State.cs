
using System;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class State : IDisposable
    {
        public IGraphAssetModel AssetModel { get; set; }

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
        }
    }
}