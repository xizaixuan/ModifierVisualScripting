using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
    public class PortMapper
    {
        private Dictionary<string, uint> inputs;
        private Dictionary<string, uint> outputs;

        public void Clear()
        {
            inputs?.Clear();
            outputs?.Clear();
        }

        public void Add(string portModelUniqueId, Direction portModelDirection, uint u)
        {
            Dictionary<string, uint> dict;
            if (portModelDirection == Direction.Input)
            {
                if (inputs == null)
                    inputs = new Dictionary<string, uint>();
                dict = inputs;
            }
            else
            {
                if (outputs == null)
                    outputs = new Dictionary<string, uint>();
                dict = outputs;
            }
            dict.Add(portModelUniqueId, u);
        }

        public int Count => (inputs == null ? 0 : inputs.Count) + (outputs == null ? 0 : outputs.Count);

        public uint GetOffset(IPortModel portModel) => GetOffset(portModel.UniqueId, portModel.Direction);
        public uint GetOffset(string portUniqueId, Direction portDirection)
        {
            return portDirection == Direction.Input ? inputs[portUniqueId] : outputs[portUniqueId];
        }

        // public override string ToString()
        // {
        //     return String.Join("\r\n",
        //         portToOffsetMapping.Select(x =>
        //             $"    Name: {x.Key} / Value: {x.Value} / PortIndex: {m_Ctx.LastPortIndex + x.Value}"));
        // }
        public IEnumerable<T> Map<T>(Func<string, Direction, uint, T> func)
        {
            return inputs == null ? Enumerable.Empty<T>() : inputs.Select(x => func(x.Key, Direction.Input, x.Value))
                .Concat(outputs == null ? Enumerable.Empty<T>() : outputs.Select(x => func(x.Key, Direction.Output, x.Value)));
        }
    }
}