
using System;
using System.IO;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.EditorCommon.Extensions;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public partial class VseWindow : GraphViewEditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/Visual Script (O)", false, 2020)]
        static void ShowNewVsEditorWindow()
        {
            ShowVsEditorWindow();
        }

        const string k_DefaultGraphAssetName = "VSGraphAsset.asset";

        Store m_Store;

        public static VseWindow ShowVsEditorWindow()
        {
            //getting all the VseWindows except derived classes
            var vseWindows = Resources.FindObjectsOfTypeAll(typeof(VseWindow)).OfExactType<VseWindow>().ToArray();
            var window = vseWindows.Length > 0 ? vseWindows[0] : CreateInstance<VseWindow>();
            window.Show();
            window.Focus();
            return window;
        }

        protected enum OpenMode { Open, OpenAndFocus }

        public void AddItemsToMenu(GenericMenu menu)
        {
            throw new System.NotImplementedException();
        }

        public static void CreateGraphAsset<TStencilType>(string graphAssetName = k_DefaultGraphAssetName, IGraphTemplate template = null)
            where TStencilType : Stencil
        {
            string uniqueFilePath = VseUtility.GetUniqueAssetPathNameInActiveFolder(graphAssetName);
            string modelName = Path.GetFileName(uniqueFilePath);

            var endAction = CreateInstance<DoCreateVisualScriptAsset>();
            endAction.Template = template;
            endAction.StencilType = typeof(TStencilType);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, endAction, modelName, GetIcon(), null);
        }

        public static Texture2D GetIcon()
        {
            return Resources.Load("visual_script_component@5x", typeof(Texture2D)) as Texture2D;
        }
    }

    class DoCreateVisualScriptAsset : EndNameEditAction
    {
        public Type StencilType { private get; set; }

        public Type GraphType
        {
            private get => m_GraphType ?? typeof(VSGraphModel);
            set => m_GraphType = value;
        }

        Type m_AssetType;
        Type m_GraphType;
        public IGraphTemplate Template;

        public Type AssetType
        {
            private get => m_AssetType ?? typeof(VSGraphAssetModel);
            set => m_AssetType = value;
        }

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var modelName = Path.GetFileNameWithoutExtension(pathName);

            var initialState = new State(null);
            var store = new Store(initialState);
            store.Dispatch(new CreateGraphAssetAction(StencilType, GraphType, AssetType, modelName, pathName, graphTemplate : Template));
            ProjectWindowUtil.ShowCreatedAsset(store.GetState().AssetModel as Object);
            store.Dispose();
        }
    }
}
