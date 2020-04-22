

using System.Text.RegularExpressions;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public static class TypeSystem
    {
        static readonly Regex k_CodifyRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);

        public static string CodifyString(string str)
        {
            return k_CodifyRegex.Replace(str, "_");
        }
    }
}