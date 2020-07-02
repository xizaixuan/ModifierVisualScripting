using Unity.Modifier.GraphElements;

namespace UnityEditor.Modifier.VisualScripting.Editor.Plugins
{
    public interface IPluginHandler
    {
        void Register(Store store, VseWindow window);
        void Unregister();

        void OptionsMenu(GenericMenu menu);
    }
}