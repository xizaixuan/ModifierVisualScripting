using Modifier.Runtime;
using System;
using System.Reflection;

namespace Modifier.DotsStencil
{
    public class TranslationSetupContext
    {
        public uint LastPortIndex;

        public IPort SetupPort(INode node, FieldInfo fieldInfo, out PortDirection direction,
            out PortType type, out string name)
        {
            var portIndex = LastPortIndex + 1;

            name = fieldInfo.Name;

            var port = (IPort)fieldInfo.GetValue(node);

            LastPortIndex += (uint)port.GetDataCount();

            var internalPort = port.GetPort();
            internalPort.Index = portIndex;

            if (port is IInputDataPort inputDataPort)
            {
                direction = PortDirection.Input;
                type = PortType.Data;
                inputDataPort.SetPort(internalPort);
                port = inputDataPort;
            }
            else if (port is IOutputDataPort outputDataPort)
            {
                direction = PortDirection.Output;
                type = PortType.Data;
                outputDataPort.SetPort(internalPort);
                port = outputDataPort;
            }
            else if (port is IInputTriggerPort inputTriggerPort)
            {
                direction = PortDirection.Input;
                type = PortType.Trigger;
                inputTriggerPort.SetPort(internalPort);
                port = inputTriggerPort;
            }
            else if (port is IOutputTriggerPort outputTriggerPort)
            {
                direction = PortDirection.Output;
                type = PortType.Trigger;
                outputTriggerPort.SetPort(internalPort);
                port = outputTriggerPort;
            }
            else
                throw new NotImplementedException();

            fieldInfo.SetValue(node, port);
            return port;
        }
    }
}