using System;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [SearcherItem(typeof(ClassStencil), SearcherContext.Graph, k_Title)]
    [Serializable]
    public class GetPropertyGroupNodeModel : PropertyGroupBaseNodeModel
    {
        public const string k_Title = "Get Property";

        public override string Title => k_Title;

        protected override Direction MemberPortDirection => Direction.Output;
    }
}