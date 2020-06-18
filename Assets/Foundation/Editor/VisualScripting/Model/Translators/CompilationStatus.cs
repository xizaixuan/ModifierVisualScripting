using JetBrains.Annotations;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    [PublicAPI]
    public enum CompilationStatus
    {
        Succeeded,
        Restart,
        Failed
    }
}