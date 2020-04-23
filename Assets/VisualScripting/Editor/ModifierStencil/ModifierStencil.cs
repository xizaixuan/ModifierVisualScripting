using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;

namespace ModifierStencil
{
    public class ModifierStencil : Stencil
    {
        public enum GraphType { Object };

        [HideInInspector, SerializeField]
        public GraphType Type;

        public ScriptingGraphAsset CompiledScriptingGraphAsset;
    }
}