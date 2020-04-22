
using System;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    // Define a way to init a graph
    public interface IGraphTemplate
    {
        Type StencilType { get; }

        void InitBasicGraph(VSGraphModel graphModel);
    }

    // Define a template that can be created from anywhere
    public interface ICreatableGraphTemplate : IGraphTemplate
    {
        string GraphTypeName { get; }

        string DefaultAssetName { get; }
    }
}