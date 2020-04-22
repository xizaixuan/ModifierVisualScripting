
using System.IO;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public static class ModelUtility
    {
        static readonly string k_AssemblyRelativePath = Path.Combine("Assets", "Runtime", "VisualScripting");

        public static string GetAssemblyRelativePath()
        {
            return k_AssemblyRelativePath;
        }
    }
}