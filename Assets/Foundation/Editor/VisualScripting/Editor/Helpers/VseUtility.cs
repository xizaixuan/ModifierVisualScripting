
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

        internal static void UpdateCodeViewer(bool show, CompilationResult compilationResult, Action<object> selectionDelegate, SourceCodePhases sourceIndex = SourceCodePhases.Initial, Type pluginIndex = null)
        {
            if (compilationResult == null)
                return;

            // Build the collection of CodeViewerCodeLine used by the CodeViewer.
            // They contain the line of code, the matching semantic node's instance ID (stored in a metadata object),
            // and a collection of messages by alert types (ex: error, warning and info)
            // The matching semantic node' instance ID is stored to allow for easy panning to the node when selecting a code line.

            IList<string> splitSourceCode = pluginIndex != null ?
                compilationResult.pluginSourceCode?[pluginIndex]?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None) :
                compilationResult.sourceCode?[(int)sourceIndex]?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (splitSourceCode == null || !splitSourceCode.Any())
                return;
            var errorPerLine = new Dictionary<int, List<CompilerError>>();
            foreach (var compilerError in compilationResult.errors)
            {
                // Line Index + Description + SemanticNode Instance Id
                var match = s_ErrorRowColumnRegex.Match(compilerError.description);
                if (!match.Success)
                    continue;

                // The line index reported is 1-indexed.
                int codeLineIndex = Convert.ToInt32(match.Groups[1].Value) - 1;

                if (!errorPerLine.ContainsKey(codeLineIndex))
                    errorPerLine[codeLineIndex] = new List<CompilerError>();
                errorPerLine[codeLineIndex].Add(compilerError);
            }


            var document = new Document(selectionDelegate);

            for (var lineIndex = 0; lineIndex < splitSourceCode.Count; lineIndex++)
            {
                string sourceCodeLine = splitSourceCode[lineIndex];

                var line = new Line(lineIndex + 1, sourceCodeLine);

                if (errorPerLine.ContainsKey(lineIndex))
                {
                    var decorator = LineDecorator.CreateError(string.Join(Environment.NewLine, errorPerLine[lineIndex].Select(error => error.description)));

                    line.AddDecorator(decorator);

                    var sourceNode = errorPerLine[lineIndex].FirstOrDefault()?.sourceNode;
                    if (sourceNode != null)
                        line.Metadata = sourceNode.Guid;
                }

                document.AddLine(line);
            }

            if (show)
                EditorWindow.GetWindow(typeof(CodeViewerWindow));

            CodeViewerWindow.SetDocument(document);
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