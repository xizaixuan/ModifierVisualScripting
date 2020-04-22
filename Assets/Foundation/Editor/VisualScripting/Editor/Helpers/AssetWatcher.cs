
using System;
using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [InitializeOnLoad]
    public class AssetWatcher : AssetPostprocessor
    {
        public static int Version;

        public class Scope : IDisposable
        {
            bool m_PreviousValue;

            public Scope()
            {
                m_PreviousValue = disabled;
                disabled = true;
            }

            public void Dispose()
            {
                disabled = m_PreviousValue;
            }
        }

        public static bool disabled;
        static AssetWatcher s_Instance;

        public static AssetWatcher Instance => s_Instance;

        Dictionary<string, string> m_ProjectAssetPaths;

        public void WatchGraphAssetAtPath(string path, GraphAssetModel graphAssetModel)
        {
            if (Instance.m_ProjectAssetPaths.ContainsKey(path))
            {
                Instance.m_ProjectAssetPaths[path] = (graphAssetModel.GraphModel as VSGraphModel)?.SourceFilePath;
            }
            else
            {
                Instance.m_ProjectAssetPaths.Add(path, (graphAssetModel.GraphModel as VSGraphModel)?.SourceFilePath);
            }
        }

        static AssetWatcher()
        {
            s_Instance = new AssetWatcher();
            Instance.m_ProjectAssetPaths = new Dictionary<string, string>();

            var graphAssetGUIDs = AssetDatabase.FindAssets("t:" + typeof(VSGraphAssetModel).Name);
            foreach (var guid in graphAssetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var graphAssetModel = AssetDatabase.LoadMainAssetAtPath(path) as GraphAssetModel;
                s_Instance.WatchGraphAssetAtPath(path, graphAssetModel);
            }
        }
    }
}