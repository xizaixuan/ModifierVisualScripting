
using System;
using System.IO;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public abstract class GraphAssetModel : ScriptableObject, IGraphAssetModel
    {
        GraphModel m_GraphModel;

        public string Name => name;

        public virtual IGraphModel GraphModel => m_GraphModel;

        public static GraphAssetModel Create(string assetName, string assetPath, Type assetTypeToCreate, bool writeOnDisk = true)
        {
            var asset = (GraphAssetModel)CreateInstance(assetTypeToCreate);
            if (!string.IsNullOrEmpty(assetPath) && writeOnDisk)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetPath) ?? "");

                if (File.Exists(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.name = assetName;
            return asset;
        }

        public GraphModel CreateGraph(Type graphTypeToCreate, string graphName, Type stencilType, bool writeOnDisk = true)
        {
            var graphModel = (GraphModel)Activator.CreateInstance(graphTypeToCreate);
            graphModel.name = graphName;
            graphModel.AssetModel = this;
            m_GraphModel = graphModel;
            if (writeOnDisk)
            {
                this.SetAssetDirty();
            }
            var stencil = (Stencil)Activator.CreateInstance(stencilType);
            Assert.IsNotNull(stencil);
            graphModel.Stencil = stencil;
            
            if (writeOnDisk)
            {
                EditorUtility.SetDirty(this);
            }
            return graphModel;
        }

        public void Dispose()
        {
        }
    }

    public static class GraphAssetModelExtensions
    {
        public static void SetAssetDirty(this IGraphAssetModel graphAssetModel)
        {
            if (graphAssetModel as Object)
            {
                EditorUtility.SetDirty((Object)graphAssetModel);
            }
        }
    }
}