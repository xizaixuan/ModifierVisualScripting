using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Modifier.Runtime
{
    [Serializable]
    public struct NodeId
    {
        [SerializeField]
        uint m_NodeIndex;

        public static NodeId Null => default;

        public NodeId(uint index)
        {
            m_NodeIndex = index + 1;
        }

        public uint GetIndex()
        {
            return m_NodeIndex - 1;
        }

        public bool IsValid()
        {
            return m_NodeIndex > 0 && m_NodeIndex < 0x7FFFFFFF;
        }

        public override string ToString()
        {
            return $"{nameof(m_NodeIndex)}: {m_NodeIndex}";
        }
    }

    [Serializable]
    public class DataId
    {
        [SerializeField]
        uint m_DataIndex;

        public static DataId Null => default;

        public DataId(uint index)
        {
            m_DataIndex = index + 1;
        }

        public uint GetIndex()
        {
            return m_DataIndex - 1;
        }

        public bool IsValid()
        {
            return m_DataIndex > 0;
        }
    }

    [Serializable]
    public struct Port : IEquatable<Port>
    {
        public uint Index;

        public static bool operator ==(Port lhs, Port rhs)
        {
            return lhs.Index == rhs.Index;
        }

        public static bool operator !=(Port lhs, Port rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(Port other)
        {
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is Port other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Index;
        }
    }

    public interface IDataPort : IPort
    {
        void SetPort(Port p);
    }

    public interface ITriggerPort : IPort
    {
        void SetPort(Port p);
    }

    public interface IMultiDataPort : IPort
    {
        void SetCount(int count);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PortDescriptionAttribute : Attribute
    {
        public string Name;
        public string Description;
        public ValueType Type;
        public object DefaultValue;

        public object ExecutionType { get; set; }

        public PortDescriptionAttribute(ValueType type, string name = null)
        {
            Type = type;
            Name = name;
        }

        public PortDescriptionAttribute(string name, ValueType type = default)
            : this(type, name)
        {
        }

        public PortDescriptionAttribute()
        {
        }
    }

    public interface IInputDataPort : IDataPort { }
    public interface IOutputDataPort : IDataPort { }
    public interface IOutputDataPort<T> : IDataPort { }
    public interface IInputTriggerPort : ITriggerPort { }
    public interface IOutputTriggerPort : ITriggerPort { }

    [Serializable]
    public struct InputDataPort : IInputDataPort
    {
        public Port Port;
        public Port GetPort() => Port;
        public int GetDataCount() => 1;
        public void SetPort(Port p) => Port = p;
        public override string ToString() => Port.Index.ToString();
    }

    [Serializable]
    public struct OutputDataPort : IOutputDataPort
    {
        public Port Port;
        public Port GetPort() => Port;
        public int GetDataCount() => 1;
        public void SetPort(Port p) => Port = p;
        public override string ToString() => Port.Index.ToString();
    }

    [Serializable]
    public struct InputTriggerPort : IInputTriggerPort, IEquatable<InputTriggerPort>
    {
        public Port Port;
        public Port GetPort() => Port;
        public void SetPort(Port p) => Port = p;
        public int GetDataCount() => 1;
        public override string ToString() => Port.Index.ToString();

        public bool Equals(InputTriggerPort other)
        {
            return Port.Equals(other.Port);
        }

        public override bool Equals(object obj)
        {
            return obj is InputTriggerPort other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Port.GetHashCode();
        }

        public static bool operator ==(InputTriggerPort left, InputTriggerPort right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputTriggerPort left, InputTriggerPort right)
        {
            return !left.Equals(right);
        }
    }

    [Serializable]
    public struct OutputTriggerPort : IOutputTriggerPort
    {
        public Port Port;
        public Port GetPort() => Port;
        public void SetPort(Port p) => Port = p;
        public int GetDataCount() => 1;
        public override string ToString() => Port.Index.ToString();
    }

    [Serializable]
    public struct InputDataMultiPort : IInputDataPort, IMultiDataPort
    {
        public Port Port;
        public int DataCount;
        public Port GetPort() => Port;
        public void SetPort(Port p) => Port = p;
        public int GetDataCount() => DataCount;
        public void SetCount(int count) => DataCount = count;
        public InputDataPort SelectPort(uint index)
        {
            Assert.IsTrue(index < DataCount);
            return new InputDataPort { Port = new Port { Index = Port.Index + index } };
        }
    }

    [Serializable]
    public struct OutputDataMultiPort : IOutputDataPort, IMultiDataPort
    {
        public Port Port;
        public int DataCount;
        public Port GetPort() => Port;
        public void SetPort(Port p) => Port = p;
        public int GetDataCount() => DataCount;
        public void SetCount(int count) => DataCount = count;
        public OutputDataPort SelectPort(uint index)
        {
            Assert.IsTrue(index < DataCount);
            return new OutputDataPort { Port = new Port { Index = Port.Index + index } };
        }
    }

    [Serializable]
    public struct InputTriggerMultiPort : IInputTriggerPort, IMultiDataPort
    {
        public Port Port;
        public int DataCount;
        public Port GetPort() => Port;
        public void SetPort(Port p) => Port = p;
        public int GetDataCount() => DataCount;
        public void SetCount(int count) => DataCount = count;
        public InputTriggerPort SelectPort(uint index)
        {
            Assert.IsTrue(index < DataCount);
            return new InputTriggerPort { Port = new Port { Index = Port.Index + index } };
        }
    }

    [Serializable]
    public struct OutputTriggerMultiPort : IOutputTriggerPort, IMultiDataPort
    {
        public Port Port;
        public int DataCount;
        public Port GetPort() => Port;
        public void SetPort(Port p) => Port = p;
        public int GetDataCount() => DataCount;
        public void SetCount(int count) => DataCount = count;
        public OutputTriggerPort SelectPort(uint index)
        {
            Assert.IsTrue(index < DataCount);
            return new OutputTriggerPort { Port = new Port { Index = Port.Index + index } };
        }
    }
}