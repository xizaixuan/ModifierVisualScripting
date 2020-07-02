using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.Modifier.GraphToolsFoundation.Model;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Modifier.GraphElements
{
    public class Port : VisualElementBridge, IGraphElement
    {
        public IGTFPortModel PortModel { get; private set; }
        public IGTFGraphElementModel Model => PortModel;
        protected IStore Store { get; private set; }
        protected ContextualMenuManipulator m_ContextualMenuManipulator;

        protected EdgeConnector m_EdgeConnector;

        [CanBeNull]
        public Label ConnectorLabel { get; protected set; }

        [CanBeNull]
        protected VisualElement ConnectorBox { get; set; }

        [CanBeNull]
        protected VisualElement ConnectorBoxCap { get; set; }

        protected GraphView GraphView { get; private set; }

        public Orientation Orientation { get; set; }

        public static readonly string k_UssClassName = "ge-port";

        public Port()
        {
            m_ContextualMenuManipulator = new ContextualMenuManipulator(BuildContextualMenu);
            this.AddManipulator(m_ContextualMenuManipulator);

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public void Setup(IGTFGraphElementModel portModel, IStore store, GraphView graphView)
        {
            PortModel = portModel as IGTFPortModel;
            Store = store;
            GraphView = graphView;

            BuildUI();
            UpdateFromModel();
        }

        protected virtual void BuildUI()
        {
            AddToClassList(k_UssClassName);
            this.AddStylesheet("Port.uss");

            if (PortModel is IHasTitle)
            {
                List<string> additionalStylesheets = new List<string>();
                additionalStylesheets.Add("PortContent/Connector");
                GraphElementsHelper.LoadTemplateAndStylesheet(this, "PortContent/LabeledConnector", "ge-port-content", additionalStylesheets);
                ConnectorLabel = this.Q<Label>("label");
                ConnectorBox = this.Q(name: "connector");
                ConnectorBoxCap = this.Q(name: "cap");
            }
            else
            {
                GraphElementsHelper.LoadTemplateAndStylesheet(this, "PortContent/Connector", "ge-port-content");
                ConnectorBox = this.Q(name: "connector");
                ConnectorBoxCap = this.Q(name: "cap");
            }

            if (ConnectorBox != null)
            {
                m_EdgeConnector = new EdgeConnector(Store, GraphView, new EdgeConnectorListener());
                ConnectorBox.AddManipulator(m_EdgeConnector);

                ConnectorBox.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
                ConnectorBox.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            }
        }

        static readonly string k_PortDataTypeClassNamePrefix = k_UssClassName + "--data-type-";

        public virtual void UpdateFromModel()
        {
            if (ConnectorLabel != null)
            {
                ConnectorLabel.text = (PortModel as IHasTitle)?.Title.Nicify() ?? String.Empty;
            }

            EnableInClassList(k_UssClassName + "--connected", PortModel.IsConnected);
            EnableInClassList(k_UssClassName + "--disconnected", !PortModel.IsConnected);

            EnableInClassList(k_UssClassName + "--direction-input", PortModel.Direction == Direction.Input);
            EnableInClassList(k_UssClassName + "--direction-output", PortModel.Direction == Direction.Output);

            this.PrefixRemoveFromClassList(k_PortDataTypeClassNamePrefix);
            AddToClassList(GetClassNameForDataType(PortModel.PortDataType));

            tooltip = PortModel.ToolTip;

            ShowHideCap();
        }

        protected virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        CustomStyleProperty<Color> m_PortColorProperty = new CustomStyleProperty<Color>("--port-color");
        void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            Color portColorValue = Color.clear;

            ICustomStyle customStyle = e.customStyle;
            if (customStyle.TryGetValue(m_PortColorProperty, out portColorValue))
                PortColor = portColorValue;
        }

        bool m_ShowCap;
        void OnMouseEnter(MouseEnterEvent evt)
        {
            m_ShowCap = true;
            ShowHideCap();
        }

        void OnMouseLeave(MouseLeaveEvent evt)
        {
            m_ShowCap = false;
            ShowHideCap();
        }

        void ShowHideCap()
        {
            if (ConnectorBoxCap != null)
            {
                if (PortModel.IsConnected || m_ShowCap)
                {
                    ConnectorBoxCap.style.visibility = StyleKeyword.Null;
                }
                else
                {
                    ConnectorBoxCap.style.visibility = Visibility.Hidden;
                }
            }
        }

        static string GetClassNameForDataType(Type thisPortType)
        {
            if (thisPortType == null)
                return String.Empty;

            if (thisPortType.IsSubclassOf(typeof(Component)))
                return k_PortDataTypeClassNamePrefix + "component";
            if (thisPortType.IsSubclassOf(typeof(GameObject)))
                return k_PortDataTypeClassNamePrefix + "game-object";
            if (thisPortType.IsSubclassOf(typeof(Rigidbody)) || thisPortType.IsSubclassOf(typeof(Rigidbody2D)))
                return k_PortDataTypeClassNamePrefix + "rigidbody";
            if (thisPortType.IsSubclassOf(typeof(Transform)))
                return k_PortDataTypeClassNamePrefix + "transform";
            if (thisPortType.IsSubclassOf(typeof(Texture)) || thisPortType.IsSubclassOf(typeof(Texture2D)))
                return k_PortDataTypeClassNamePrefix + "texture2d";
            if (thisPortType.IsSubclassOf(typeof(KeyCode)))
                return k_PortDataTypeClassNamePrefix + "key-code";
            if (thisPortType.IsSubclassOf(typeof(Material)))
                return k_PortDataTypeClassNamePrefix + "material";
            if (thisPortType == typeof(Object))
                return k_PortDataTypeClassNamePrefix + "object";
            return k_PortDataTypeClassNamePrefix + thisPortType.Name.ToKebabCase();
        }

        public EdgeConnector EdgeConnector => m_EdgeConnector;

        public bool WillConnect
        {
            set
            {
                m_ShowCap = value;
                EnableInClassList("ge-port--will-connect", value);
                ShowHideCap();
            }
        }

        public bool Highlighted
        {
            set
            {
                EnableInClassList("ge-port--highlighted", value);
                foreach (var edgeModel in PortModel.ConnectedEdges)
                {
                    var edge = edgeModel.GetUI<Edge>(GraphView);
                    edge?.EdgeControl.MarkDirtyRepaint();
                }
            }
        }

        Node m_Node;

        public Node node
        {
            get
            {
                if (m_Node == null)
                {
                    m_Node = GetFirstAncestorOfType<Node>();
                }

                return m_Node;
            }
        }

        public Vector3 GetGlobalCenter()
        {
            Vector2 overriddenPosition;

            if (GraphView != null && GraphView.GetPortCenterOverride(this, out overriddenPosition))
            {
                return overriddenPosition;
            }

            return ConnectorBox.LocalToWorld(ConnectorBox.GetRect().center);
        }

        public Color PortColor { get; private set; }
    }
}