using System;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IGraphModel : IDisposable
    {
        string Name { get; }

        IGraphAssetModel AssetModel { get; }

        Stencil Stencil { get; }
    }
}