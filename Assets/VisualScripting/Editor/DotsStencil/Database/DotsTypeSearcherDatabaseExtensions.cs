using System;
using System.Linq;
using Modifier.Runtime;
using UnityEditor.Searcher;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    static class DotsTypeSearcherDatabaseExtensions
    {
        internal static TypeSearcherDatabase AddBasicDotsTypes(this TypeSearcherDatabase self)
        {
            self.RegisterTypes(items => items.AddRange(
                Enum.GetValues(typeof(ValueType))
                    .Cast<ValueType>()
                    .Where(x => x != ValueType.Unknown)
                    .Select(x =>
                    {
                        var valueTypeToTypeHandle = x == ValueType.Entity ? TypeHandle.GameObject : x.ValueTypeToTypeHandle();
                        return (SearcherItem)new TypeSearcherItem(
                            valueTypeToTypeHandle,
                            x.FriendlyName());
                    })));
            return self;
        }

        internal static TypeSearcherDatabase AddTypesInheritingFrom<T>(this TypeSearcherDatabase self)
        {
            var baseType = typeof(T);
            self.RegisterTypesFromMetadata((items, metadata) =>
            {
                if (!metadata.IsAssignableTo(baseType))
                    return false;
                var classItem = new TypeSearcherItem(metadata.TypeHandle, metadata.FriendlyName);
                return items.TryAddClassItem(self.Stencil, classItem, metadata);
            });
            return self;
        }
    }
}
