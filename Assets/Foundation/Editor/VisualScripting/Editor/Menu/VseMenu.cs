﻿using UnityEditor.Modifier.EditorCommon;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public partial class VseMenu : Toolbar
    {
        protected readonly Store m_Store;
        readonly VseGraphView m_GraphView;

        public VseMenu(Store store, VseGraphView graphView)
        {
            m_Store = store;
            m_GraphView = graphView;
            name = "vseMenu";
            AddToClassList("gtf-toolbar");
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(PackageTransitionHelper.AssetPath + "VisualScripting/Editor/Menu/VseMenu.uss"));

            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PackageTransitionHelper.AssetPath + "VisualScripting/Editor/Menu/VseMenu.uxml").CloneTree(this);

            CreateCommonMenu();
            CreateBreadcrumbMenu();
            //CreateTracingMenu();
            CreateOptionsMenu();
        }

        public virtual void UpdateUI()
        {
            VSPreferences prefs = m_Store.GetState().Preferences;
            bool isEnabled = m_Store.GetState().CurrentGraphModel != null;
            UpdateCommonMenu(prefs, isEnabled);
            UpdateBreadcrumbMenu(isEnabled);
        }
    }
}