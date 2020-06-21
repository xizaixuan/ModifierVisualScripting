﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;
using Port = Unity.Modifier.GraphElements.Port;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    [Serializable]
    public abstract class NodeModel : INodeModel, ISerializationCallbackReceiver, IHasProgress, IHasTitle, ICollapsible, IHasPorts, IGTFNodeModel
    {
        [SerializeField]
        string m_GuidAsString; // serialize m_Guid

        GUID m_Guid; // serialized as m_GuidAsString

        [SerializeField]
        GraphAssetModel m_GraphAssetModel;

        IStackModel m_ParentStackModel;

        [SerializeField]
        Vector2 m_Position;

        [SerializeField, HideInInspector]
        Color m_Color;

        [SerializeField, HideInInspector]
        bool m_HasUserColor;

        [SerializeField]
        string m_Title;

        // Serialize m_InputConstantsById dictionary Keys
        [SerializeField, HideInInspector]
        List<string> m_InputConstantKeys;

        // Serialize m_InputConstantsById dictionary Values
        [SerializeReference]
        protected List<ConstantNodeModel> m_InputConstantsValues;

        [SerializeField]
        ModelState m_State;

        public GUID Guid
        {
            get => m_Guid;
            internal set => m_Guid = value;
        }

        protected Stencil Stencil => m_GraphAssetModel != null ? m_GraphAssetModel.GraphModel.Stencil : null;

        public virtual string IconTypeString => "typeNode";

        public virtual string DataTypeString
        {
            get
            {
                IVariableDeclarationModel declarationModel = (this as IVariableModel)?.DeclarationModel;
                return declarationModel?.DataType.GetMetadata(Stencil).FriendlyName ?? string.Empty;
            }
        }

        public virtual string VariableString
        {
            get
            {
                IVariableDeclarationModel declarationModel = (this as IVariableModel)?.DeclarationModel;
                return declarationModel == null ? string.Empty : declarationModel.IsExposed ? "Exposed variable" : "Variable";
            }
        }

        public ScriptableObject SerializableAsset => (ScriptableObject)VSGraphModel.AssetModel;

        public IGraphAssetModel AssetModel
        {
            get => m_GraphAssetModel;
            set
            {
                Assert.IsNotNull(value);
                m_GraphAssetModel = (GraphAssetModel)value;
            }
        }

        public IGTFGraphModel GraphModel => AssetModel?.GraphModel;
        public IGraphModel VSGraphModel => GraphModel as IGraphModel;

        public ModelState State
        {
            get => m_State;
            set => m_State = value;
        }

        public IStackModel ParentStackModel
        {
            get => m_ParentStackModel;
            set => m_ParentStackModel = value;
        }

        public virtual string Title
        {
            get => m_Title;
            set => m_Title = value;
        }

        public Vector2 Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        public virtual bool IsCondition => false;
        public virtual bool IsInsertLoop => false;
        public virtual LoopConnectionType LoopConnectionType => LoopConnectionType.None;

        public bool IsBranchType => GetType().GetCustomAttribute<BranchedNodeAttribute>() != null;

        public Color Color
        {
            get => m_HasUserColor ? m_Color : Color.clear;
            set => m_Color = value;
        }

        // Allows maintaining a ports both by order and by their ids
        // IReadOnlyList<IPortModel> gives access to ports by display order
        // IReadOnlyDictionary<string, IPortModel> gives access to ports by Ids
        [PublicAPI]
        protected class OrderedPorts : IReadOnlyDictionary<string, IPortModel>, IReadOnlyList<IPortModel>
        {
            Dictionary<string, IPortModel> m_Dictionary;
            List<int> m_Order;
            List<IPortModel> m_PortModels;

            public OrderedPorts(int capacity = 0)
            {
                m_Dictionary = new Dictionary<string, IPortModel>(capacity);
                m_Order = new List<int>(capacity);
                m_PortModels = new List<IPortModel>(capacity);
            }

            public void Add(IPortModel portModel)
            {
                m_Dictionary.Add(portModel.UniqueId, portModel);
                m_PortModels.Add(portModel);
                m_Order.Add(m_Order.Count);
            }

            public bool Remove(IPortModel portModel)
            {
                bool found = false;
                if (m_Dictionary.ContainsKey(portModel.UniqueId))
                {
                    m_Dictionary.Remove(portModel.UniqueId);
                    found = true;
                    int index = m_PortModels.FindIndex(x => x == portModel);
                    m_PortModels.Remove(portModel);
                    m_Order.Remove(index);
                    for (int i = 0; i < m_Order.Count; ++i)
                    {
                        if (m_Order[i] > index)
                            --m_Order[i];
                    }
                }
                return found;
            }

            public void SwapPortsOrder(IPortModel a, IPortModel b)
            {
                int indexA = m_PortModels.IndexOf(a);
                int indexB = m_PortModels.IndexOf(b);
                int oldAOrder = m_Order[indexA];
                m_Order[indexA] = m_Order[indexB];
                m_Order[indexB] = oldAOrder;
            }

            #region IReadOnlyDictionary implementation
            public IEnumerator<KeyValuePair<string, IPortModel>> GetEnumerator() => m_Dictionary.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => m_Dictionary.Count;
            public bool ContainsKey(string key) => m_Dictionary.ContainsKey(key);

            public bool TryGetValue(string key, out IPortModel value)
            {
                return m_Dictionary.TryGetValue(key, out value);
            }

            public IPortModel this[string key] => m_Dictionary[key];

            public IEnumerable<string> Keys => m_Dictionary.Keys;
            public IEnumerable<IPortModel> Values => m_Dictionary.Values;
            #endregion IReadOnlyDictionary implementation

            #region IReadOnlyList<IPortModel> implementation
            IEnumerator<IPortModel> IEnumerable<IPortModel>.GetEnumerator()
            {
                Assert.AreEqual(m_Order.Count, m_PortModels.Count, "these lists are supposed to always be of the same size");
                return m_Order.Select(i => m_PortModels[i]).GetEnumerator();
            }

            public IPortModel this[int index] => m_PortModels[m_Order[index]];
            #endregion IReadOnlyList<IPortModel> implementation
        }

        OrderedPorts m_InputsById;
        OrderedPorts m_OutputsById;
        protected OrderedPorts m_PreviousInputs;
        protected OrderedPorts m_PreviousOutputs;

        Dictionary<string, ConstantNodeModel> m_InputConstantsById;
        public NodeModel()
        {
            m_OutputsById = new OrderedPorts();
            m_InputsById = new OrderedPorts();
            m_Color = new Color(0.776f, 0.443f, 0, 0.5f);
        }

        public IReadOnlyDictionary<string, IPortModel> InputsById => m_InputsById;
        public IReadOnlyDictionary<string, IPortModel> OutputsById => m_OutputsById;

        public IReadOnlyDictionary<string, ConstantNodeModel> InputConstantsById => m_InputConstantsById;

        public virtual bool IsStacked => m_ParentStackModel != null;
        public virtual IReadOnlyList<IPortModel> InputsByDisplayOrder => m_InputsById;

        public virtual IReadOnlyList<IPortModel> OutputsByDisplayOrder => m_OutputsById;

        [Obsolete("Direct indexing dropped, use InputsById or InputsByDisplayOrder instead")]
        public virtual IReadOnlyList<IPortModel> InputPortModels => new List<IPortModel>();
        [Obsolete("Direct indexing dropped, use OutputsById or OutputsByDisplayOrder instead")]
        public virtual IReadOnlyList<IPortModel> OutputPortModels => new List<IPortModel>();

        public virtual bool Collapsed { get; set; }
        public IEnumerable<IGTFPortModel> InputPorts => m_InputsById.Values.Cast<IGTFPortModel>();
        public IEnumerable<IGTFPortModel> OutputPorts => m_OutputsById.Values.Cast<IGTFPortModel>();

        public bool HasUserColor
        {
            get => m_HasUserColor;
            set => m_HasUserColor = value;
        }

        public virtual bool HasProgress => false;

        public virtual void OnConnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
        }

        public virtual void OnDisconnection(IPortModel selfConnectedPortModel, IPortModel otherConnectedPortModel)
        {
        }

        public void PostGraphLoad()
        {
            if (m_Guid.Empty())
                m_Guid = GUID.Generate();
        }

        protected virtual void OnPreDefineNode()
        {
        }

        public void DefineNode()
        {
            OnPreDefineNode();

            m_PreviousInputs = m_InputsById;
            m_PreviousOutputs = m_OutputsById;
            m_InputsById = new OrderedPorts(m_InputsById?.Count ?? 0);
            m_OutputsById = new OrderedPorts(m_OutputsById?.Count ?? 0);

            OnDefineNode();

            RemoveUnusedPorts();
        }

        void RemoveUnusedPorts()
        {
            foreach (var kv in m_PreviousInputs
                     .Where<KeyValuePair<string, IPortModel>>(kv => !m_InputsById.ContainsKey(kv.Key)))
            {
                DeletePort(kv.Value);
            }

            foreach (var kv in m_PreviousOutputs
                     .Where<KeyValuePair<string, IPortModel>>(kv => !m_OutputsById.ContainsKey(kv.Key)))
            {
                DeletePort(kv.Value);
            }

            if (m_InputConstantsById == null)
            {
                m_InputConstantsById = new Dictionary<string, ConstantNodeModel>();
                m_InputConstantsById.DeserializeDictionaryFromLists(m_InputConstantKeys, m_InputConstantsValues);
            }

            // remove input constants that aren't used
            var idsToDeletes = m_InputConstantsById
                .Select(kv => kv.Key)
                .Where(id => !m_InputsById.ContainsKey(id)).ToList();
            foreach (var id in idsToDeletes)
            {
                m_InputConstantsById[id].Destroy();
                m_InputConstantsById.Remove(id);
            }
        }

        static PortModel ReuseOrCreatePortModel(PortModel model, IReadOnlyDictionary<string, IPortModel> previousPorts, OrderedPorts newPorts)
        {
            // reuse existing ports when ids match, otherwise add port
            string id = model.UniqueId;
            PortModel modelToAdd = model;
            if (previousPorts.TryGetValue(id, out var existingModel))
            {
                modelToAdd = (PortModel)existingModel;
                modelToAdd.Name = model.Name;
                modelToAdd.DataTypeHandle = model.DataTypeHandle;
                modelToAdd.PortType = model.PortType;
            }
            newPorts.Add(modelToAdd);
            return modelToAdd;
        }

        protected virtual void OnDefineNode()
        {
        }

        public virtual void UndoRedoPerformed()
        {
            Profiler.BeginSample("NodeModel_UndoRedo");
            DefineNode();
            Profiler.EndSample();
        }

        public virtual PortCapacity GetPortCapacity(PortModel portModel)
        {
            return Stencil.GetPortCapacity(portModel, out var cap) ? cap : portModel?.GetDefaultCapacity() ?? PortCapacity.Multi;
        }

        public string GetId()
        {
            return Guid.ToString();
        }

        public void AssignNewGuid()
        {
            Guid = GUID.Generate();
        }

        public void Move(Vector2 position)
        {
            Position = position;
        }

        public void ChangeColor(Color color)
        {
            HasUserColor = true;
            m_Color = color;
        }

        protected virtual PortModel MakePortForNode(Direction direction, string portName, PortType portType, TypeHandle dataType, string portId, PortModel.PortModelOptions options, object portCreationdata)
        {
            return new PortModel(portName ?? "", portId, options)
            {
                Direction = direction,
                PortType = portType,
                DataTypeHandle = dataType,
                NodeModel = this
            };
        }

        protected PortModel AddDataInputPort<TDataType>(string portName, string portId = null, PortModel.PortModelOptions options = PortModel.PortModelOptions.Default, TDataType defaultValue = default)
        {
            Action<ConstantNodeModel> preDefine = null;

            if (!EqualityComparer<TDataType>.Default.Equals(defaultValue, default))
                preDefine = constantModel => constantModel.SetValue(defaultValue);

            return AddDataInputPort(portName, typeof(TDataType).GenerateTypeHandle(Stencil), portId, options, preDefine);
        }

        protected PortModel AddDataInputPort(string portName, TypeHandle typeHandle, string portId = null, PortModel.PortModelOptions options = PortModel.PortModelOptions.Default, Action<ConstantNodeModel> preDefine = null)
        {
            return AddInputPort(portName, PortType.Data, typeHandle, portId, options, preDefine);
        }

        protected PortModel AddDataOutputPort<TDataType>(string portName, string portId = null)
        {
            return AddDataOutputPort(portName, typeof(TDataType).GenerateTypeHandle(Stencil), portId);
        }

        protected PortModel AddDataOutputPort(string portName, TypeHandle typeHandle, string portId = null)
        {
            return AddOutputPort(portName, PortType.Data, typeHandle, portId);
        }

        protected PortModel AddInstanceInput<TDataType>(string portName = null, string portId = null)
        {
            return AddInstanceInput(typeof(TDataType).GenerateTypeHandle(Stencil), portName, portId);
        }

        protected PortModel AddInstanceInput(TypeHandle dataType, string portName = null, string portId = null)
        {
            return AddInputPort(portName, PortType.Instance, dataType, portId);
        }

        protected PortModel AddExecutionInputPort(string portName, string portId = null)
        {
            return AddInputPort(portName, PortType.Execution, TypeHandle.ExecutionFlow, portId);
        }

        protected PortModel AddExecutionOutputPort(string portName, string portId = null)
        {
            return AddOutputPort(portName, PortType.Execution, TypeHandle.ExecutionFlow, portId);
        }

        protected PortModel AddLoopOutputPort(string portName, string portId = null)
        {
            return AddOutputPort(portName, PortType.Loop, TypeHandle.ExecutionFlow, portId);
        }

        protected virtual PortModel AddInputPort(string portName, PortType portType, TypeHandle dataType, string portId = null, PortModel.PortModelOptions options = PortModel.PortModelOptions.Default, Action<ConstantNodeModel> preDefine = null, object portCreationData = null)
        {
            var portModel = MakePortForNode(Direction.Input, portName, portType, dataType, portId, options, portCreationData);
            portModel = ReuseOrCreatePortModel(portModel, m_PreviousInputs, m_InputsById);
            UpdateConstantForInput(portModel, preDefine);
            return portModel;
        }

        protected virtual PortModel AddOutputPort(string portName, PortType portType, TypeHandle dataType, string portId = null, PortModel.PortModelOptions options = PortModel.PortModelOptions.Default, object portCreationData = null)
        {
            var portModel = MakePortForNode(Direction.Output, portName, portType, dataType, portId, options, portCreationData);
            return ReuseOrCreatePortModel(portModel, m_PreviousOutputs, m_OutputsById);
        }

        void UpdateConstantForInput(PortModel inputPort, Action<ConstantNodeModel> preDefine = null)
        {
            var id = inputPort.UniqueId;
            if ((inputPort.Options & PortModel.PortModelOptions.NoEmbeddedConstant) != 0)
            {
                m_InputConstantsById?.Remove(id);
                return;
            }
            if (m_InputConstantsById == null)
            {
                m_InputConstantsById = new Dictionary<string, ConstantNodeModel>();
                m_InputConstantsById.DeserializeDictionaryFromLists(m_InputConstantKeys, m_InputConstantsValues);
            }

            if (m_InputConstantsById.TryGetValue(id, out var constant))
            {
                // Destroy existing constant if not compatible
                Type type = inputPort.DataTypeHandle.Resolve(Stencil);
                constant.AssetModel = m_GraphAssetModel;
                if (!constant.Type.IsAssignableFrom(type))
                {
                    m_InputConstantsById.Remove(id);
                }
            }

            // Create new constant if needed
            if (!m_InputConstantsById.ContainsKey(id)
                && inputPort.CreateEmbeddedValueIfNeeded
                && inputPort.DataTypeHandle != TypeHandle.Unknown
                && Stencil.GetConstantNodeModelType(inputPort.DataTypeHandle) != null)
            {
                var embeddedConstant = (ConstantNodeModel)((VSGraphModel)GraphModel).CreateConstantNode(
                    inputPort.Name,
                    inputPort.DataTypeHandle,
                    Vector2.zero,
                    SpawnFlags.Default | SpawnFlags.Orphan,
                    preDefine: preDefine);
                Utility.SaveAssetIntoObject(embeddedConstant, (Object)AssetModel);
                m_InputConstantsById[id] = embeddedConstant;
            }
        }

        internal void ReinstantiateInputConstants()
        {
            foreach (var id in m_InputConstantsById.Keys.ToList())
            {
                ConstantNodeModel inputConstant = m_InputConstantsById[id];
                ConstantNodeModel newConstant = inputConstant.Clone();
                m_InputConstantsById[id] = newConstant;
                Utility.SaveAssetIntoObject(newConstant, (Object)AssetModel);
            }
        }

        protected void DeleteInputPort(PortModel portModel)
        {
            DeletePort(portModel);
        }

        protected void DeleteOutputPort(PortModel portModel)
        {
            DeletePort(portModel);
        }

        protected void DeletePort(IPortModel portModel, bool removeFromOrderedPorts = false)
        {
            var edgeModels = VSGraphModel.GetEdgesConnections(portModel);
            ((GraphModel)GraphModel).DeleteEdges(edgeModels);
            if (removeFromOrderedPorts)
            {
                if (m_InputsById.Remove(portModel))
                {
                    m_PreviousInputs.Remove(portModel);
                }
                else if (m_OutputsById.Remove(portModel))
                {
                    m_PreviousOutputs.Remove(portModel);
                }
            }
        }

        public bool Destroyed { get; private set; }

        public void Destroy() => Destroyed = true;

        public virtual string ToolTip => string.Empty;

        public virtual void OnBeforeSerialize()
        {
            m_InputConstantsById.SerializeDictionaryToLists(ref m_InputConstantKeys, ref m_InputConstantsValues);
            m_GuidAsString = m_Guid.ToString();
        }

        public virtual void OnAfterDeserialize()
        {
            m_OutputsById = new OrderedPorts();
            m_InputsById = new OrderedPorts();

            if (!GUID.TryParse(m_GuidAsString, out m_Guid))
                m_Guid = default;
        }

        public static implicit operator bool(NodeModel n)
        {
            return n != null;
        }

        public virtual bool IsDeletable => true;
        public virtual bool IsDroppable => true;
        public virtual bool IsCopiable => true;
    }

    [Serializable]
    public abstract class HighLevelNodeModel : NodeModel { }
}