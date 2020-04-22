
using System.Reflection;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public static class VseUtility
    {
        public static void MarkForUpdate(this State state, UpdateFlags flag)
        {
            state.EditorDataModel?.SetUpdateFlag(flag);
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
    }
}