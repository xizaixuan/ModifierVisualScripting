using Modifier.Elements;
using Unity.Modifier.GraphElements;
using UnityEditor;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Editor.Plugins;
using UnityEngine.UIElements;

namespace Modifier.DotsStencil
{
    public class DotsBoundObjectPlugin : IPluginHandler
    {
        private const string k_WarningPlayMode = "Warning: during Play Mode, scene references cannot be edited";
        private const string k_WarningEditAsset = "Warning: when editing a project asset and not a scene object, scene references cannot be edited";
        private Store m_Store;
        private VseWindow m_Window;
        private Label m_WarningLabel;

        public void Register(Store store, VseWindow window)
        {
            m_Store = store;
            m_Window = window;
            EditorApplication.update += Update;
            CreateWarningLabel();
            Update();
        }

        private void CreateWarningLabel()
        {
            if (m_WarningLabel == null)
            {
                m_WarningLabel = new Label();
                m_WarningLabel.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UIHelper.TemplatePath + "WarningLabel.uss"));
                m_WarningLabel.AddToClassList("dots-warning-label");
            }
        }

        private void ShowWarningLabel(string content)
        {
            if (m_Window == null)
                return;
            CreateWarningLabel();
            if (m_WarningLabel.parent != m_Window.GraphView)
                m_Window.GraphView.Add(m_WarningLabel);
            m_WarningLabel.text = content;
        }

        private void HideWarningLabel()
        {
            if (m_Window == null)
                return;
            m_WarningLabel?.RemoveFromHierarchy();
        }

        private void Update()
        {
//             if (m_Store == null)
//                 return;
//             if (EditorApplication.isPlaying)
//                 ShowWarningLabel(k_WarningPlayMode);
//             else
//             {
//                 var dotsStencil = (DotsStencil)m_Store.GetState().CurrentGraphModel?.Stencil;
//                 if (dotsStencil != null && dotsStencil.Type == DotsStencil.GraphType.Object && (!(m_Store?.GetState()?.EditorDataModel?.BoundObject is UnityEngine.Object obj) || !obj))
//                     ShowWarningLabel(k_WarningEditAsset);
//                 else
//                     HideWarningLabel();
//             }
        }

        public void Unregister()
        {
            EditorApplication.update -= Update;
            HideWarningLabel();
            m_Store = null;
            m_Window = null;
        }

        public void OptionsMenu(GenericMenu menu)
        {
        }
    }
}
