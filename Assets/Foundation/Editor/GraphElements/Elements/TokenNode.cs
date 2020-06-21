using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class TokenNode : Node
    {
        PortContainer m_InputPortContainer;
        PortContainer m_OutputPortContainer;

        public Texture Icon { get; set; }

        protected override void BuildUI()
        {
            base.BuildUI();

            this.AddStylesheet("TokenNode.uss");
            AddToClassList(k_UssClassName + "--token");

            Debug.Assert(NodeModel is IHasSingleInputPort || NodeModel is IHasSingleOutputPort);

            if (NodeModel is IHasPorts)
            {
                this.AddStylesheet("PortTopContainer.uss");

                m_InputPortContainer = new PortContainer { name = "inputs" };
                m_InputPortContainer.AddToClassList("ge-node__inputs");
                Insert(0, m_InputPortContainer);

                m_OutputPortContainer = new PortContainer { name = "outputs" };
                m_OutputPortContainer.AddToClassList("ge-node__outputs");
                Add(m_OutputPortContainer);
            }
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            if (TitleLabel != null)
            {
                TitleLabel.Text = (NodeModel as IHasTitle)?.Title.Nicify() ?? String.Empty;
            }

            if (NodeModel is IHasSingleInputPort inputPortHolder && inputPortHolder.GTFInputPort != null)
            {
                Debug.Assert(inputPortHolder.GTFInputPort.Direction == Direction.Input);
                m_InputPortContainer?.UpdatePorts(new[] { inputPortHolder.GTFInputPort }, GraphView, Store);
            }
            if (NodeModel is IHasSingleOutputPort outputPortHolder && outputPortHolder.GTFOutputPort != null)
            {
                Debug.Assert(outputPortHolder.GTFOutputPort.Direction == Direction.Output);
                m_OutputPortContainer?.UpdatePorts(new[] { outputPortHolder.GTFOutputPort }, GraphView, Store);
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
    }
}