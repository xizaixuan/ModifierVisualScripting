using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Searcher;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    [PublicAPI]
    public static class SearcherItemCollectionExtensions
    {
        const string k_Enums = "Enumerations";
        const string k_Class = "Classes";
        const string k_Graphs = "Graphs";

        public static void AddAtPath(this List<SearcherItem> items, SearcherItem item, string path = "")
        {
            if (!string.IsNullOrEmpty(path))
            {
                SearcherItem parent = SearcherItemUtility.GetItemFromPath(items, path);
                parent.AddChild(item);
            }
            else
            {
                items.Add(item);
            }
        }

        public static bool TryAddEnumItem(
            this List<SearcherItem> items,
            SearcherItem itemToAdd,
            ITypeMetadata meta,
            string parentName = ""
        )
        {
            if (meta.IsEnum)
            {
                items.AddAtPath(itemToAdd, parentName + "/" + k_Enums);
                return true;
            }

            return false;
        }

        public static bool TryAddClassItem(
            this List<SearcherItem> items,
            Stencil stencil,
            SearcherItem itemToAdd,
            ITypeMetadata meta,
            string parentName = ""
        )
        {
            if ((meta.IsClass || meta.IsValueType) && !meta.IsEnum)
            {
                var path = BuildPath(parentName + "/" + k_Class, meta);
                items.AddAtPath(itemToAdd, path);
                return true;
            }

            return false;
        }

        static string BuildPath(string parentName, ITypeMetadata meta)
        {
            return parentName + "/" + meta.Namespace.Replace(".", "/");
        }
    }
}