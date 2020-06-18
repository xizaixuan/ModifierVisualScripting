
using System;
using System.Collections.Generic;
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

        public static void MarkForUpdate(this State state, UpdateFlags flag)
        {
            state.EditorDataModel?.SetUpdateFlag(flag);
        }

        public static Capabilities ConvertCapabilities(ICapabilitiesModel model)
        {
            Assert.IsNotNull(model);
            Capabilities capabilities = 0;
            if (model.Capabilities.HasFlag(CapabilityFlags.Ascendable))
                capabilities |= Capabilities.Ascendable;
            if (model.Capabilities.HasFlag(CapabilityFlags.Collapsible))
                capabilities |= Capabilities.Collapsible;
            if (model.Capabilities.HasFlag(CapabilityFlags.Deletable))
                capabilities |= Capabilities.Deletable;
            if (model.Capabilities.HasFlag(CapabilityFlags.Movable))
                capabilities |= Capabilities.Movable;
            if (model.Capabilities.HasFlag(CapabilityFlags.Resizable))
                capabilities |= Capabilities.Resizable;
            if (model.Capabilities.HasFlag(CapabilityFlags.Selectable))
                capabilities |= Capabilities.Selectable;
            if (model.Capabilities.HasFlag(CapabilityFlags.Droppable))
                capabilities |= Capabilities.Droppable;
            if (model.Capabilities.HasFlag(CapabilityFlags.Renamable))
                capabilities |= Capabilities.Renamable;
            if (model.Capabilities.HasFlag(CapabilityFlags.Copiable))
                capabilities |= Capabilities.Copiable;
            return capabilities;
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

        public static void AddTokenIcon(GraphElement tokenElement, LoopStackModel.TitleComponentIcon titleComponentIcon)
        {
            if (!(tokenElement is Token) && !(tokenElement is TokenDeclaration))
                return;

            var tokenElementIcon = tokenElement.Q("icon");
            if (tokenElementIcon == null)
                return;

            switch (titleComponentIcon)
            {
                case LoopStackModel.TitleComponentIcon.Collection:
                    tokenElementIcon.AddToClassList("typeArray");
                    tokenElementIcon.style.visibility = StyleKeyword.Null;
                    break;
                case LoopStackModel.TitleComponentIcon.Condition:
                    tokenElementIcon.AddToClassList("typeBoolean");
                    tokenElementIcon.style.visibility = StyleKeyword.Null;
                    break;
                case LoopStackModel.TitleComponentIcon.Count:
                    tokenElementIcon.AddToClassList("typeCount");
                    tokenElementIcon.style.visibility = StyleKeyword.Null;
                    break;
                case LoopStackModel.TitleComponentIcon.Index:
                    tokenElementIcon.AddToClassList("typeIndex");
                    tokenElementIcon.style.visibility = StyleKeyword.Null;
                    break;
                case LoopStackModel.TitleComponentIcon.Item:
                    tokenElementIcon.AddToClassList("typeItem");
                    tokenElementIcon.style.visibility = StyleKeyword.Null;
                    break;
                case LoopStackModel.TitleComponentIcon.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(titleComponentIcon), titleComponentIcon, null);
            }
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