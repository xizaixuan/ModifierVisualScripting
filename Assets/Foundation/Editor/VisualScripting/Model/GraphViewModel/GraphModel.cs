using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public abstract class GraphModel : IGraphModel
    {
        [SerializeField]
        GraphAssetModel m_AssetModel;

        [SerializeReference]
        Stencil m_Stencil;

        public virtual string Name => name;

        public IGraphAssetModel AssetModel
        {
            get => m_AssetModel;
            set => m_AssetModel = (GraphAssetModel)value;
        }

        public string name;

        public Stencil Stencil
        {
            get => m_Stencil;
            set => m_Stencil = value;
        }

        public void Dispose()
        {
        }
    }
}