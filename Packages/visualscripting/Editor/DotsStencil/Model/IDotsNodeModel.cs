using Modifier.Runtime;
using System;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    public interface IDotsNodeModel : INodeModel
    {
        Type NodeType { get; }
        INode Node { get; }
        PortMapper PortToOffsetMapping { get; }
    }
}