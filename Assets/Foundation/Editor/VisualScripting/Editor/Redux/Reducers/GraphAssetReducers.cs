
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    static class GraphAssetReducers
    {
        public static void Register(Store store)
        {
            store.Register<CreateGraphAssetAction>(CreateGraphAsset);
        }

        static State CreateGraphAsset(State previousState, CreateGraphAssetAction action)
        {
            previousState.AssetModel?.Dispose();
            using (new AssetWatcher.Scope())
            {
                GraphAssetModel graphAssetModel = GraphAssetModel.Create(action.Name, action.AssetPath, action.AssetType, action.WriteOnDisk);

                var graphModel = graphAssetModel.CreateGraph(action.GraphType, action.Name, action.StencilType, action.WriteOnDisk);
                if (action.GraphTemplate != null)
                {
                    action.GraphTemplate.InitBasicGraph(graphModel as VSGraphModel);
                }

                previousState.AssetModel = graphAssetModel;
            }

            if (action.WriteOnDisk)
            {
                AssetDatabase.SaveAssets();
            }

            AssetWatcher.Instance.WatchGraphAssetAtPath(action.AssetPath, (GraphAssetModel)previousState.AssetModel);
            previousState.MarkForUpdate(UpdateFlags.All);

            return previousState;
        }
    }
}