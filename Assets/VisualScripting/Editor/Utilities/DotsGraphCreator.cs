using Modifier.DotsStencil;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Editor;

namespace Modifier.VisualScripting.Editor
{
    public static class DotsGraphCreator
    {
        [MenuItem("Assets/Create/VisualScripting/Modifier Scripting Graph")]
        public static void GreateGraph(MenuCommand menuCommand)
        {
            var initialState = new State(null);
            var store = new Store(initialState);
            var template = DotsGraphTemplate.ObjectGraphAsset();
            template.PromptToCreate(store);
        }
    }
}