using System;
using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.Editor.Plugins;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class State : Unity.Modifier.GraphElements.State
    {
        public IGraphModel CurrentGraphModel => GraphModel as IGraphModel;

        public VSPreferences Preferences => EditorDataModel?.Preferences;

        public new IEditorDataModel EditorDataModel => base.EditorDataModel as IEditorDataModel;

        public ICompilationResultModel CompilationResultModel { get; private set; }

        /// <summary>
        /// Stores the list of steps for the current graph, frame and target tuple
        /// </summary>
        public List<TracingStep> DebuggingData { get; set; }

        public int CurrentTracingTarget = -1;
        public int CurrentTracingFrame;
        public int CurrentTracingStep;
        public int MaxTracingStep;

        public enum UIRebuildType                             // for performance debugging purposes
        {
            None, Partial, Full
        }
        public string LastDispatchedActionName { get; set; }    // ---
        public UIRebuildType lastActionUIRebuildType;           // ---

        public State(IEditorDataModel editorDataModel)
        {
            CompilationResultModel = new CompilationResultModel();
            base.EditorDataModel = editorDataModel;
            CurrentTracingStep = -1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnloadCurrentGraphAsset();
                CompilationResultModel = null;
                DebuggingData = null;
            }

            base.Dispose(disposing);
        }

        ~State()
        {
            Dispose(false);
        }

        public void UnloadCurrentGraphAsset()
        {
            AssetModel?.Dispose();
            AssetModel = null;
            if (EditorDataModel != null)
            {
                //TODO: should not be needed ?
                EditorDataModel.PluginRepository?.UnregisterPlugins();
            }
        }

        public void RegisterReducers(Store store, Action clearRegistrations)
        {
            clearRegistrations();
            store.RegisterReducers();
            CurrentGraphModel?.Stencil?.RegisterReducers(store);
        }
    }
}