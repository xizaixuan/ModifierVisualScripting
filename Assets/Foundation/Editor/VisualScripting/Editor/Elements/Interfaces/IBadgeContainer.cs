using Unity.Modifier.GraphElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface IBadgeContainer
    {
        IconBadge ErrorBadge { get; set; }
        ValueBadge ValueBadge { get; set; }
    }
}