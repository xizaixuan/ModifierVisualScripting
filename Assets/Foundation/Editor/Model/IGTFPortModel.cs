using System;
using System.Collections.Generic;
using Unity.Modifier.GraphElements;

namespace Unity.Modifier.GraphToolsFoundation.Model
{
    public enum PortCapacity
    {
        Single,
        Multi
    }

    public interface IGTFPortModel : IGTFGraphElementModel
    {
        IGTFNodeModel NodeModel { get; }
        Direction Direction { get; }
        Orientation Orientation { get; }
        PortCapacity Capacity { get; }
        Type PortDataType { get; }
        bool IsConnected { get; }
        bool IsConnectedTo(IGTFPortModel port);
        IEnumerable<IGTFEdgeModel> ConnectedEdges { get; }
        string ToolTip { get; }
    }
}
