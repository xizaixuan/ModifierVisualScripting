
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Modifier.VisualScripting;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public static class VseUtility
    {
        static Regex s_ErrorRowColumnRegex = new Regex(@"\(([^)]*),([^)]*)\)", RegexOptions.Compiled);

        public static bool IsPrefabOrAsset(Object obj)
        {
            return EditorUtility.IsPersistent(obj) || (obj.hideFlags & HideFlags.NotEditable) != 0;
        }

        public static string GetUniqueAssetPathNameInActiveFolder(string filename)
        {
            string path;
            try
            {
                // Private implementation of a file naming function which puts the file at the selected path.
                var assetDatabase = typeof(AssetDatabase);
                path = (string)assetDatabase.GetMethod("GetUniquePathNameAtSelectedPath", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(assetDatabase, new object[] { filename });
            }
            catch
            {
                // Protection against implementation changes.
                path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + filename);
            }
            return path;
        }

        public static GUIContent CreatTextContent(string content)
        {
            // TODO: Replace by EditorGUIUtility.TrTextContent when it's made 'public'.
            return new GUIContent(content);
        }

        public static string GetTitle(MethodBase methodInfo)
        {
            if (methodInfo != null && methodInfo.IsConstructor)
                return "New " + methodInfo.DeclaringType?.Name.Nicify();
            var attribute = methodInfo?.GetCustomAttribute<VisualScriptingFriendlyNameAttribute>();
            return (attribute?.FriendlyName ?? methodInfo?.Name)?.Nicify();
        }

        public static string GetTitle(Type type)
        {
            var attribute = type?.GetCustomAttribute<VisualScriptingFriendlyNameAttribute>();
            return (attribute?.FriendlyName ?? type?.Name)?.Nicify();
        }

        public static void LogSticky(LogType logType, LogOption logOptions, string message)
        {
            LogSticky(logType, logOptions, message, string.Empty, 0);
        }

        public static void LogSticky(LogType logType, LogOption logOptions, string message, string file, int instanceId)
        {
            ConsoleWindowBridge.LogSticky(message, file, logType, logOptions, instanceId);
        }

        public static void RemoveLogEntries()
        {
            ConsoleWindowBridge.RemoveLogEntries();
        }

        public static void SetupLogStickyCallback()
        {
            ConsoleWindowBridge.SetEntryDoubleClickedDelegate((file, entryInstanceId) =>
            {
                string[] pathAndGuid = file.Split('@');
                VseWindow window = VseWindow.OpenVseAssetInWindow(pathAndGuid[0]);
                if (GUID.TryParse(pathAndGuid[1], out GUID guid))
                    window.Store?.Dispatch(new PanToNodeAction(guid));
            });
        }
    }
}