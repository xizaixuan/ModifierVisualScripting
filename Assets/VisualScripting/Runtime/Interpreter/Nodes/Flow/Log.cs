using System;
using Modifier.Runtime.Nodes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription(LogType.Message, "Write **Message** in the unity console. You can right click on the node to add more than one message.\n" + "\n" + "**Warning: You should remove the Logs when you make a standalone.**")]
    public struct Log : IFlowNode, IHasExecutionType<Log.LogType>
    {
        public enum LogType
        {
            Message,
            Warning,
            Error,
        }
        [PortDescription("", Description = "Trigger the writing in the Unity Console.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute next action after the message is written in the Unity Console.")]
        public OutputTriggerPort Output;
        [PortDescription(ValueType.StringReference, Description = "The message you want to write. Placing a GameObject will write it's name.")]
        public InputDataMultiPort Messages;

        [SerializeField]
        private LogType _type;

        public LogType Type
        {
            get => _type;
            set => _type = value;
        }

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            Assert.AreEqual(Input.Port.Index, port.Port.Index);
            string message = null;

            for (uint i = 0; i < Messages.DataCount; i++)
            {
                ConcatToMessage(ctx, ctx.ReadValue(Messages.SelectPort(i)), ref message);
            }

            if (message != null)
            {
                switch (Type)
                {
                    case LogType.Message:
                        Debug.Log(message);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(message);
                        break;
                    case LogType.Error:
                        Debug.LogError(message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ctx.Trigger(Output);
        }

        static void ConcatToMessage<TCtx>(TCtx ctx, Value value, ref string message) where TCtx : IGraphInstance
        {
            if (message == null)
                message = "";
            switch (value.Type)
            {
                case ValueType.StringReference:
                    message += ctx.GetString(value.StringReference);
                    break;
                case ValueType.Entity:
                    message += ctx.GetString(value.Entity);
                    break;
                default:
                    message += value.ToString();
                    break;
            }
        }
    }
}
