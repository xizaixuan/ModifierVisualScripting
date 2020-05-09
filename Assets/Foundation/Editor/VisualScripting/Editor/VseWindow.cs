
using System;
using System.IO;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Callbacks;
using UnityEditor.Modifier.EditorCommon;
using UnityEditor.Modifier.EditorCommon.Extensions;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public partial class VseWindow : GraphViewEditorWindow, IHasCustomMenu
    {
        const string k_StyleSheetPath = PackageTransitionHelper.AssetPath + "VisualScripting/Editor/Views/Templates/";

        const string k_DefaultGraphAssetName = "VSGraphAsset.asset";

        // Window itself

        Store m_Store;

        VisualElement m_GraphContainer;

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

        [OnOpenAsset(1)]
        public static bool OpenVseAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is VSGraphAssetModel)
            {
                string path = AssetDatabase.GetAssetPath(instanceId);
                return OpenVseAssetInWindow(path) != null;
            }

            return false;
        }

        public static VseWindow OpenVseAssetInWindow(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VSGraphAssetModel>(path);
            if (asset == null)
            {
                return null;
            }

            VseWindow vseWindow = ShowVsEditorWindow();

            vseWindow.SetCurrentSelection(path, OpenMode.OpenAndFocus);

            return vseWindow;
        }

        protected virtual void OnEnable()
        {
            var ttttt = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath + "VSEditor.uss");
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(k_StyleSheetPath + "VSEditor.uss"));
            rootVisualElement.Clear();
            rootVisualElement.style.overflow = Overflow.Hidden;
            rootVisualElement.pickingMode = PickingMode.Ignore;
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.name = "vseRoot";

            m_GraphContainer = new VisualElement { name = "graphContainer" };
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            var disabled = m_Store?.GetState().CurrentGraphModel == null;

            m_LockTracker.AddItemsToMenu(menu, disabled);
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
