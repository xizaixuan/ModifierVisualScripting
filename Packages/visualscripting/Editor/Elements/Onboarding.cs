using JetBrains.Annotations;
using Modifier.Runtime;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;
using UnityEngine.UIElements;
using Modifier.VisualScripting.Editor;

namespace Modifier.Elements
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    class DotsOnboardingProvider : VSOnboardingProvider
    {
        public VisualElement CreateOnboardingElement(Store store)
        {
            return new Onboarding(store);
        }

        public bool GetGraphAndObjectFromSelection(VseWindow vseWindow, Object selectedObject,
            out string assetPath, out GameObject boundObject)
        {
            assetPath = null;
            boundObject = null;

            if (selectedObject is IGraphAssetModel graphAssetModel)
            {
                // don't change the current object if it's the same graph
                if (graphAssetModel == vseWindow.CurrentGraphModel?.AssetModel)
                {
                    assetPath = vseWindow.LastGraphFilePath;
                    boundObject = vseWindow.BoundObject;
                    return true;
                }
            }

            ScriptingGraphAuthoring authoring;
            if (!(selectedObject is GameObject gameObject) ||
                (!(authoring = gameObject.GetComponent<ScriptingGraphAuthoring>())))
                return false;

            var path = AssetDatabase.GetAssetPath(authoring.ScriptingGraph);
            if (path == null)
                return false;

            assetPath = path;
            boundObject = selectedObject as GameObject;

            return true;
        }
    }

    class Onboarding : VisualElement
    {
        public Onboarding(Store store)
        {
            VisualTreeAsset template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIHelper.TemplatePath + "Onboarding.uxml");
            template.CloneTree(this);

#if !UNITY_2020_1_OR_NEWER
            AddToClassList("onboarding-block");
            AddToClassList("dots-onboarding");
#endif

            var button = this.Q<Button>("create-with-object");
            button.clicked += () =>
            {
                DotsGraphCreator.CreateGraphOnNewGameObject(store, null, true);
            };

            button = this.Q<Button>("create-from-void");
            button.clicked += () =>
            {
                DotsGraphCreator.CreateGraphOnNewGameObject(store, null, false);
            };

            button = this.Q<Button>("create-subgraph");
            button.clicked += () =>
            {
                DotsGraphCreator.CreateSubgraph(store);
            };

            RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
            RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);
        }

        void OnEnterPanel(AttachToPanelEvent e)
        {
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        void OnLeavePanel(DetachFromPanelEvent e)
        {
            // ReSharper disable once DelegateSubtraction
            Selection.selectionChanged -= OnSelectionChanged;
        }

        void OnSelectionChanged()
        {
            this.Q<Button>("create-with-object")?.SetEnabled(Selection.gameObjects.Length > 0);
        }
    }
}
