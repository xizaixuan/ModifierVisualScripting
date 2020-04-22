
using System;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IGraphAssetModel : IDisposable
    {
        string Name { get; }

        IGraphModel GraphModel { get; }
    }
}