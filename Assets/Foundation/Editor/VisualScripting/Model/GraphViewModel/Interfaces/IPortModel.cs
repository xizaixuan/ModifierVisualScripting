﻿using System;
using System.Collections.Generic;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine.UIElements;
using Port = Unity.Modifier.GraphElements.Port;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IPortModel : IGraphElementModel
    {
        string Name { get; }
        string UniqueId { get; }
        INodeModel NodeModel { get; }
        ConstantNodeModel EmbeddedValue { get; }
        Action<IChangeEvent, Store, IPortModel> EmbeddedValueEditorValueChangedOverride { get; set; }
        bool CreateEmbeddedValueIfNeeded { get; }

        IEnumerable<IPortModel> ConnectionPortModels { get; }

        Direction Direction { get; }
        PortType PortType { get; }
        TypeHandle DataType { get; }
        Port.Capacity Capacity { get; }
        bool Connected { get; }
        Action OnValueChanged { get; set; }
        string ToolTip { get; }
    }
}