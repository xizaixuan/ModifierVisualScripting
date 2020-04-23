using UnityEngine;

namespace Modifier.Runtime
{
    public class ScriptingGraphAsset : ScriptableObject
    {
        [SerializeReference]
        public GraphDefinition Definition;

        [SerializeField]
        public uint HashCode;
    }
}