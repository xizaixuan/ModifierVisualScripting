using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    partial class VseMenu
    {
        ToolbarBreadcrumbs m_Breadcrumb;

        void CreateBreadcrumbMenu()
        {
            m_Breadcrumb = this.MandatoryQ<ToolbarBreadcrumbs>("breadcrumb");
        }

        void UpdateBreadcrumbMenu(bool isEnabled)
        {
            m_Breadcrumb.SetEnabled(isEnabled);

            State state = m_Store.GetState();
            IGraphModel graphModel = state.CurrentGraphModel;

            m_Breadcrumb.TrimItems(0);

            int i = 0;
            for (; i < state.EditorDataModel.PreviousGraphModels.Count; i++)
            {
                var graphToLoad = state.EditorDataModel.PreviousGraphModels[i];
                string label = graphToLoad.GraphAssetModel && graphToLoad.GraphAssetModel.GraphModel != null ? graphToLoad.GraphAssetModel.GraphModel.FriendlyScriptName : "<Unknown>";
                int i1 = i;
                m_Breadcrumb.CreateOrUpdateItem(i, label, () =>
                {
                    while (state.EditorDataModel.PreviousGraphModels.Count > i1)
                        state.EditorDataModel.PreviousGraphModels.RemoveAt(state.EditorDataModel.PreviousGraphModels.Count - 1);
                    m_Store.Dispatch(new LoadGraphAssetAction(graphToLoad.GraphAssetModel.GraphModel.GetAssetPath(), loadType: LoadGraphAssetAction.Type.KeepHistory));
                });
            }

            string newCurrentGraph = graphModel?.FriendlyScriptName;
            if (newCurrentGraph != null)
            {
                m_Breadcrumb.CreateOrUpdateItem(i++, newCurrentGraph, null);
            }

            m_Breadcrumb.TrimItems(i);
        }
    }
}