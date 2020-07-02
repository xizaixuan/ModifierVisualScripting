using Unity.Modifier.GraphElements;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    public class SearcherGraphView : GraphView
    {
        public SearcherGraphView(Store store) : base(store)
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "SearcherGraphView.uss"));

            contentContainer.style.flexBasis = StyleKeyword.Auto;

            AddToClassList("searcherGraphView");

            UnregisterCallback<ValidateCommandEvent>(OnValidateCommand);
            UnregisterCallback<ExecuteCommandEvent>(OnExecuteCommand);
            RegisterCallback<AttachToPanelEvent>(OnEnterPanel);
        }

        void OnEnterPanel(AttachToPanelEvent e)
        {
            base.OnEnterPanel();

            panel.visualTree.UnregisterCallback<KeyDownEvent>(OnKeyDownShortcut);
        }
    }
}