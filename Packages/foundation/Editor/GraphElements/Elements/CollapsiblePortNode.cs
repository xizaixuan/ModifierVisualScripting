using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphToolsFoundation.Model;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class CollapsiblePortNode : Node
    {
        [CanBeNull]
        public PortContainer InputPortContainer { get; protected set; }
        [CanBeNull]
        public PortContainer OutputPortContainer { get; protected set; }
        [CanBeNull]
        protected VisualElement CollapseButton { get; set; }

        static readonly string k_CollapsedUssClassName = k_UssClassName + "--collapsed";
        static readonly string k_NotConnectedUssClassName = k_UssClassName + "--not-connected";

        protected override void BuildUI()
        {
            base.BuildUI();

            if (NodeModel is ICollapsible)
            {
                CollapseButton = new CollapseButton { name = "collapse-button" };
                CollapseButton.AddToClassList(k_UssClassName + "__collapse-button");
                CollapseButton.RegisterCallback<ChangeEvent<bool>>(OnCollapse);

                if (TitleContainer != null)
                {
                    TitleContainer.Add(CollapseButton);
                }
                else
                {
                    Add(CollapseButton);
                }
            }

            if (NodeModel is IHasPorts)
            {
                this.AddStylesheet("PortTopContainer.uss");

                var ports = new VisualElement { name = "port-top-container" };
                ports.AddToClassList(k_UssClassName + "__port-top-container");
                Add(ports);

                InputPortContainer = new PortContainer { name = "inputs" };
                InputPortContainer.AddToClassList("ge-node__inputs");
                ports.Add(InputPortContainer);

                OutputPortContainer = new PortContainer { name = "outputs" };
                OutputPortContainer.AddToClassList("ge-node__outputs");
                ports.Add(OutputPortContainer);
            }
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            EnableInClassList(k_CollapsedUssClassName, Collapsed);

            if (NodeModel is IHasPorts portHolder)
            {
                bool noPortConnected = true;
                bool allInputConnected = true;
                foreach (var port in portHolder.InputPorts)
                {
                    if (!port.IsConnected)
                    {
                        allInputConnected = false;
                    }
                    else
                    {
                        noPortConnected = false;
                    }

                    if (!allInputConnected && !noPortConnected)
                        break;
                }

                bool allOutputConnected = true;
                foreach (var port in portHolder.OutputPorts)
                {
                    if (!port.IsConnected)
                    {
                        allOutputConnected = false;
                    }
                    else
                    {
                        noPortConnected = false;
                    }

                    if (!allOutputConnected && !noPortConnected)
                        break;
                }

                CollapseButton?.SetDisabledPseudoState(allInputConnected && allOutputConnected);

                EnableInClassList(k_NotConnectedUssClassName, noPortConnected);

                InputPortContainer?.UpdatePorts(portHolder.InputPorts, GraphView, Store);
                OutputPortContainer?.UpdatePorts(portHolder.OutputPorts, GraphView, Store);
            }
        }

        protected override void UpdateEdgeLayout()
        {
            if (NodeModel is IHasPorts portContainer)
            {
                foreach (var portModel in portContainer.InputPorts)
                {
                    foreach (var edgeModel in portModel.ConnectedEdges)
                    {
                        var edge = edgeModel.GetUI<Edge>(GraphView);
                        edge?.EdgeControl.PointsChanged();
                        edge?.UpdateEdgeControl();
                    }
                }
                foreach (var portModel in portContainer.OutputPorts)
                {
                    foreach (var edgeModel in portModel.ConnectedEdges)
                    {
                        var edge = edgeModel.GetUI<Edge>(GraphView);
                        edge?.EdgeControl.PointsChanged();
                        edge?.UpdateEdgeControl();
                    }
                }
            }
        }

        public override void MarkEdgesDirty()
        {
            if (NodeModel is IHasPorts portContainer)
            {
                foreach (var portModel in portContainer.InputPorts)
                {
                    foreach (var edgeModel in portModel.ConnectedEdges)
                    {
                        var edge = edgeModel.GetUI<Edge>(GraphView);
                        edge?.EdgeControl.PointsChanged();
                    }
                }
                foreach (var portModel in portContainer.OutputPorts)
                {
                    foreach (var edgeModel in portModel.ConnectedEdges)
                    {
                        var edge = edgeModel.GetUI<Edge>(GraphView);
                        edge?.EdgeControl.PointsChanged();
                    }
                }
            }
        }

        bool Collapsed
        {
            get => (NodeModel as ICollapsible)?.Collapsed ?? false;
            set => Store.Dispatch(new SetNodeCollapsedAction(NodeModel, value));
        }

        // PF: remove this; only used in tests
        public void RefreshPorts()
        {
            if (NodeModel is IHasPorts portHolder)
            {
                InputPortContainer?.UpdatePorts(portHolder.InputPorts, GraphView, Store);
                OutputPortContainer?.UpdatePorts(portHolder.OutputPorts, GraphView, Store);
            }
        }

        void OnCollapse(ChangeEvent<bool> e)
        {
            Collapsed = e.newValue;
        }

        protected override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target == this)
            {
                evt.menu.AppendAction("Disconnect all", DisconnectAll, DisconnectAllStatus);
                evt.menu.AppendSeparator();
            }
        }

        static void AddConnectionsToDeleteSet(IEnumerable<IGTFPortModel> ports, ref HashSet<IGTFGraphElementModel> toDelete)
        {
            if (ports == null)
                return;

            foreach (var port in ports)
            {
                if (port.IsConnected)
                {
                    foreach (var c in port.ConnectedEdges)
                    {
                        if (!c.IsDeletable)
                            continue;

                        toDelete.Add(c);
                    }
                }
            }
        }

        void DisconnectAll(DropdownMenuAction a)
        {
            if (NodeModel is IHasPorts portHolder)
            {
                HashSet<IGTFGraphElementModel> toDeleteModels = new HashSet<IGTFGraphElementModel>();

                AddConnectionsToDeleteSet(portHolder.InputPorts, ref toDeleteModels);
                AddConnectionsToDeleteSet(portHolder.OutputPorts, ref toDeleteModels);

                Store.Dispatch(new DeleteElementsAction(toDeleteModels.ToArray()));
            }
        }

        DropdownMenuAction.Status DisconnectAllStatus(DropdownMenuAction a)
        {
            if (NodeModel is IHasPorts portHolder &&
                portHolder.InputPorts.Concat(portHolder.OutputPorts).Any(port => port.IsConnected))
            {
                return DropdownMenuAction.Status.Normal;
            }

            return DropdownMenuAction.Status.Disabled;
        }
    }
}