using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Modifier.EditorCommon;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public static class GraphElementsHelper
    {
        public static void LoadTemplateAndStylesheet(VisualElement container, string name, string rootClassName, IEnumerable<string> additionalStylesheets = null)
        {
            if (name != null && container != null)
            {
                var tpl = LoadUXML(name + ".uxml");
                tpl.CloneTree(container);

                if (additionalStylesheets != null)
                {
                    foreach (var additionalStylesheet in additionalStylesheets)
                    {
                        container.AddStylesheet(additionalStylesheet + ".uss");
                    }
                }

                container.AddStylesheet(name + ".uss");

#if !UNITY_2020_1_OR_NEWER
                container.AddToClassList(rootClassName);
#endif
            }
        }

        public static string StylesheetPath = PackageTransitionHelper.AssetPath + "GraphElements/Stylesheets/";
        public static string TemplatePath = PackageTransitionHelper.AssetPath + "GraphElements/Templates/";

        public static void AddStylesheet(this VisualElement ve, string stylesheetName)
        {
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylesheetPath + stylesheetName);
            if (stylesheet != null)
            {
                ve.styleSheets.Add(stylesheet);
            }
            else
            {
                Debug.Log("Failed to load stylesheet " + StylesheetPath + stylesheetName);
            }
        }

        public static VisualTreeAsset LoadUXML(string uxmlName)
        {
            var tpl = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath + uxmlName);
            if (tpl == null)
            {
                Debug.Log("Failed to load template " + TemplatePath + uxmlName);
            }

            return tpl;
        }
    }
}