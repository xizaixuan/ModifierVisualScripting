using System;

namespace Modifier.Runtime
{
    [Serializable]
    public struct PortInfo
    {
        public bool IsDataPort; // True if this is a dataport, false if it's a trigger port
        public bool IsOutputPort; // True if this is an output port
        public uint DataIndex; // Index in the Data table for data port, and in the trigger table for trigger ports
        public NodeId NodeId; // The node ID owning that port
        public string PortName; // Port name (for debug only)

        public override string ToString()
        {
            string idxName = IsDataPort ? "DataIndex" : "TriggerIndex";
            return $"{idxName}: {DataIndex}, {nameof(NodeId)}: {NodeId}, {nameof(PortName)}: {PortName}";
        }
    }
}