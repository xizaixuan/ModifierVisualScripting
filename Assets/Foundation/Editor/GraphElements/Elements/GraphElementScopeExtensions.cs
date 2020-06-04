using System;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;

namespace Unity.Modifier.GraphElements
{
    public static class GraphElementScopeExtensions
    {
        static readonly PropertyName containingScopePropertyKey = "containingScope";

        public static Scope GetContainingScope(this GraphElement element)
        {
            if (element == null)
                return null;

            return element.GetProperty(containingScopePropertyKey) as Scope;
        }

        internal static void SetContainingScope(this GraphElement element, Scope scope)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetProperty(containingScopePropertyKey, scope);
        }
    }
}