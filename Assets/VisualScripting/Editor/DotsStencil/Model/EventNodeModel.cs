using JetBrains.Annotations;
using Modifier.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;

namespace Modifier.DotsStencil
{
    [UsedImplicitly]
    public interface IEventNodeModel : IDotsNodeModel, IPropertyVisitorNodeTarget
    {
        TypeHandle TypeHandle { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    static class IEventNodeModelExtensions
    {
        public static IEnumerable<BaseDotsNodeModel.PortMetaData> GetPortsMetaData(
            this IEventNodeModel self,
            Stencil stencil)
        {
            var type = self.TypeHandle.Resolve(stencil);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                var fieldHandle = self is SendEventNodeModel && field.FieldType == typeof(NativeString128)
                    ? TypeHandle.String
                    : field.FieldType.GenerateTypeHandle(stencil);

                yield return new BaseDotsNodeModel.PortMetaData
                {
                    Name = field.Name,
                    Type = fieldHandle.ToValueType()
                };
            }
        }
    }

    [Serializable]
    class SendEventNodeModel : DotsNodeModel<SendEvent>, IEventNodeModel, IHasMainExecutionInputPort,
        IHasMainExecutionOutputPort
    {
        [SerializeField]
        TypeHandle m_TypeHandle;

        public TypeHandle TypeHandle
        {
            get => m_TypeHandle;
            set => m_TypeHandle = value;
        }

        public override string Title => $"Send {TypeHandle.Name(Stencil)}";
        public IPortModel ExecutionInputPort { get; set; }
        public IPortModel ExecutionOutputPort { get; set; }

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData =>
            new Dictionary<string, List<PortMetaData>>
        {
            { nameof(SendEvent.Values), new List<PortMetaData>(this.GetPortsMetaData(Stencil)) },
        };
    }

    [Serializable]
    class OnEventNodeModel : DotsNodeModel<OnEvent>, IEventNodeModel, IHasMainExecutionOutputPort
    {
        [SerializeField]
        TypeHandle m_TypeHandle;

        public TypeHandle TypeHandle
        {
            get => m_TypeHandle;
            set => m_TypeHandle = value;
        }

        public override string Title => $"On {TypeHandle.Name(Stencil)}";
        public IPortModel ExecutionOutputPort { get; set; }

        public override IReadOnlyDictionary<string, List<PortMetaData>> PortCustomData =>
            new Dictionary<string, List<PortMetaData>>
        {
            { nameof(OnEvent.Values), new List<PortMetaData>(this.GetPortsMetaData(Stencil)) },
        };
    }
}