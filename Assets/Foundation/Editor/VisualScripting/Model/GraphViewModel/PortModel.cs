using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = System.Object;
using Port = Unity.Modifier.GraphElements.Port;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public class PortModel : IPortModel, IHasTitle, IGTFPortModel
    {
        [Flags]
        public enum PortModelOptions
        {
            None = 0,
            NoEmbeddedConstant = 1,
            Default = None,
        }

        public static bool Equivalent(IPortModel a, IPortModel b)
        {
            if (a == null || b == null)
                return a == b;
            return a.Direction == b.Direction && a.NodeModel.Guid == b.NodeModel.Guid && a.UniqueId == b.UniqueId;
        }

        public PortModelOptions Options { get; protected set; }

        TypeHandle m_DataType;

        public PortModel(string name = null, string uniqueId = null, PortModelOptions options = PortModelOptions.Default)
        {
            m_Name = name;
            m_UniqueId = uniqueId;
            Options = options;
        }

        public ScriptableObject SerializableAsset => (ScriptableObject)NodeModel.VSGraphModel.AssetModel;
        public IGraphAssetModel AssetModel => VSGraphModel?.AssetModel;
        public IGraphModel VSGraphModel => NodeModel?.VSGraphModel;
        public IGTFGraphModel GraphModel => VSGraphModel as IGTFGraphModel;

        string m_Name;

        string m_UniqueId;

        public string UniqueId => m_UniqueId ?? m_Name ?? "";

        public string Name
        {
            get => m_Name;
            set
            {
                if (value == m_Name)
                    return;
                m_Name = value;
                OnValueChanged?.Invoke();
            }
        }

        public string Title
        {
            get => Name;
            set => Name = value;
        }

        INodeModel m_NodeModel;

        public INodeModel NodeModel
        {
            get => m_NodeModel as INodeModel;
            set
            {
                if (value == m_NodeModel)
                    return;

                m_NodeModel = value;
                OnValueChanged?.Invoke();
            }
        }

        IGTFNodeModel IGTFPortModel.NodeModel => m_NodeModel as IGTFNodeModel;

        public ConstantNodeModel EmbeddedValue
        {
            get
            {
                if (NodeModel is NodeModel node && node.InputConstantsById.TryGetValue(UniqueId, out var inputModel))
                {
                    return inputModel;
                }

                return null;
            }
        }

        public Action<IChangeEvent, Store, IPortModel> EmbeddedValueEditorValueChangedOverride { get; set; }

        public virtual bool CreateEmbeddedValueIfNeeded => PortType == PortType.Data;

        public IEnumerable<IPortModel> ConnectionPortModels
        {
            get { Assert.IsNotNull(GraphModel, $"portModel {Name} has a null GraphModel reference"); return VSGraphModel.GetConnections(this); }
        }

        PortType m_PortType;

        public PortType PortType
        {
            get => m_PortType;
            set
            {
                if (value == m_PortType)
                    return;

                m_PortType = value;
                OnValueChanged?.Invoke();
            }
        }

        Direction m_Direction;

        public Direction Direction
        {
            get => m_Direction;
            set
            {
                if (value == m_Direction)
                    return;

                m_Direction = value;
                OnValueChanged?.Invoke();
            }
        }

        // PF: Is Orientation ever set?
        public Orientation Orientation { get; set; }

        // Give node model priority over self
        public PortCapacity Capacity => NodeModel?.GetPortCapacity(this) ?? GetDefaultCapacity();

        public bool IsConnected => ConnectionPortModels.Any();
        public bool IsConnectedTo(IGTFPortModel port)
        {
            var edgeModels = VSGraphModel.EdgeModels.Where(e =>
                e.InputPortModel == this && e.OutputPortModel == port ||
                e.OutputPortModel == this && e.InputPortModel == port);
            return edgeModels.Any();
        }

        IEnumerable<IGTFEdgeModel> IGTFPortModel.ConnectedEdges => ConnectedEdges.Cast<IGTFEdgeModel>();

        public IEnumerable<IEdgeModel> ConnectedEdges => VSGraphModel.EdgeModels.Where(e => e.InputPortModel == this || e.OutputPortModel == this);

        public Action OnValueChanged { get; set; }

        public TypeHandle DataTypeHandle
        {
            get
            {
                return m_DataType;
            }
            set
            {
                if (value == m_DataType)
                    return;

                m_DataType = value;
                OnValueChanged?.Invoke();
            }
        }

        public Type PortDataType
        {
            get
            {
                var stencil = VSGraphModel.Stencil;
                Type t = DataTypeHandle.Resolve(stencil);
                t = t == typeof(void) || t.ContainsGenericParameters ? typeof(Unknown) : t;
                return t;
            }
        }

        public override string ToString()
        {
            return $"Port {NodeModel}: {PortType} {Name}(id: {UniqueId ?? "\"\""})";
        }

        public PortCapacity GetDefaultCapacity()
        {
            return (PortType == PortType.Data || PortType == PortType.Instance) ?
                Direction == Direction.Input ?
                PortCapacity.Single :
                PortCapacity.Multi :
                (PortType == PortType.Execution || PortType == PortType.Loop) ?
                Direction == Direction.Output ?
                PortCapacity.Single :
                PortCapacity.Multi :
                PortCapacity.Multi;
        }

        public string GetId()
        {
            return string.Empty;
        }

        public string IconTypeString
        {
            get
            {
                Stencil stencil = NodeModel.VSGraphModel.Stencil;

                // TODO: should TypHandle.Resolve do this for us?
                // @THEOR SAID HE WOULD THINK ABOUT IT (written on CAPS DAY 2018)
                if (NodeModel is EnumConstantNodeModel enumConst)
                {
                    Type t = enumConst.EnumType.Resolve(stencil);
                    return "type" + t.Name;
                }

                Type thisPortType = DataTypeHandle.Resolve(stencil);

                if (thisPortType.IsSubclassOf(typeof(Component)))
                    return "typeComponent";
                if (thisPortType.IsSubclassOf(typeof(GameObject)))
                    return "typeGameObject";
                if (thisPortType.IsSubclassOf(typeof(Rigidbody)) || thisPortType.IsSubclassOf(typeof(Rigidbody2D)))
                    return "typeRigidBody";
                if (thisPortType.IsSubclassOf(typeof(Transform)))
                    return "typeTransform";
                if (thisPortType.IsSubclassOf(typeof(Texture)) || thisPortType.IsSubclassOf(typeof(Texture2D)))
                    return "typeTexture2D";
                if (thisPortType.IsSubclassOf(typeof(KeyCode)))
                    return "typeKeycode";
                if (thisPortType.IsSubclassOf(typeof(Material)))
                    return "typeMaterial";
                if (thisPortType == typeof(Object))
                    return "typeObject";
                return "type" + thisPortType.Name;
            }
        }

        public virtual string ToolTip
        {
            get
            {
                string newTooltip = Direction == Direction.Output ? "Output" : "Input";
                switch (PortType)
                {
                    case PortType.Execution:
                        newTooltip += " execution flow";
                        if (NodeModel.IsCondition)
                            newTooltip += $" ({Name.ToLower()} condition)";
                        break;
                    case PortType.Loop:
                        newTooltip += " loop";
                        break;
                    case PortType.Data:
                    case PortType.Instance:
                        var stencil = VSGraphModel.Stencil;
                        newTooltip += $" of type {(DataTypeHandle == TypeHandle.ThisType ? (NodeModel?.VSGraphModel)?.FriendlyScriptName : DataTypeHandle.GetMetadata(stencil).FriendlyName)}";
                        break;
                    case PortType.Event:
                        newTooltip += " event";
                        break;
                }
                return newTooltip;
            }
        }
    }
}