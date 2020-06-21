using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class Edge : GraphElement, IMovable
    {
        const float k_EndPointRadius = 4.0f;
        const float k_InterceptWidth = 6.0f;
        static CustomStyleProperty<int> s_EdgeWidthProperty = new CustomStyleProperty<int>("--edge-width");
        static CustomStyleProperty<Color> s_SelectedEdgeColorProperty = new CustomStyleProperty<Color>("--selected-edge-color");
        static CustomStyleProperty<Color> s_EdgeColorProperty = new CustomStyleProperty<Color>("--edge-color");

        static readonly int k_DefaultEdgeWidth = 2;
        static readonly Color k_DefaultSelectedColor = new Color(240 / 255f, 240 / 255f, 240 / 255f);
        static readonly Color k_DefaultColor = new Color(146 / 255f, 146 / 255f, 146 / 255f);

        static readonly string k_EditModeClassName = "edge--edit-mode";

        public IGTFEdgeModel EdgeModel => Model as IGTFEdgeModel;

        protected ContextualMenuManipulator m_ContextualMenuManipulator;

        public VisualElement Container { get; }

        protected bool IsGhostEdge => EdgeModel is IGhostEdge;

        public Vector2 From
        {
            get
            {
                var p = Vector2.zero;

                var port = EdgeModel.FromPort;
                if (port == null)
                {
                    if (EdgeModel is IGhostEdge ghostEdgeModel)
                    {
                        p = ghostEdgeModel.EndPoint;
                    }
                }
                else
                {
                    var ui = port.GetUI<Port>(GraphView);
                    if (ui == null)
                        return Vector2.zero;

                    p = ui.GetGlobalCenter();
                }

                return this.WorldToLocal(p);
            }
        }

        public Vector2 To
        {
            get
            {
                var p = Vector2.zero;

                var port = EdgeModel.ToPort;
                if (port == null)
                {
                    if (EdgeModel is GhostEdgeModel ghostEdgeModel)
                    {
                        p = ghostEdgeModel.EndPoint;
                    }
                }
                else
                {
                    var ui = port.GetUI<Port>(GraphView);
                    if (ui == null)
                        return Vector2.zero;

                    p = ui.GetGlobalCenter();
                }

                return this.WorldToLocal(p);
            }
        }

        public IGTFPortModel Output => EdgeModel.FromPort;

        public IGTFPortModel Input => EdgeModel.ToPort;

        public override bool ShowInMiniMap => false;

        EdgeControl m_EdgeControl;
        public EdgeControl EdgeControl
        {
            get
            {
                if (m_EdgeControl == null)
                {
                    m_EdgeControl = new EdgeControl
                    {
                        interceptWidth = k_InterceptWidth
                    };
                    Add(m_EdgeControl);
                }
                return m_EdgeControl;
            }
        }

        public int EdgeWidth { get; set; } = k_DefaultEdgeWidth;

        public Color SelectedColor { get; set; } = k_DefaultSelectedColor;

        public Color DefaultColor { get; set; } = k_DefaultColor;

        public Edge()
        {
            GraphElementsHelper.LoadTemplateAndStylesheet(this, "Edge", "ge-edge");

            Container = this.Q("container");
            Debug.Assert(Container != null);

            this.AddManipulator(new EdgeManipulator());

            m_ContextualMenuManipulator = new ContextualMenuManipulator(BuildContextualMenu);
            this.AddManipulator(m_ContextualMenuManipulator);

            RegisterCallback<AttachToPanelEvent>(OnEdgeAttach);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();
            EnableInClassList(k_EditModeClassName, EdgeModel.EditMode);
            EdgeControl.PointsChanged();
            EdgeControl.RebuildControlPointsUI();

            OnPortChanged();
        }

        protected virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }

        public override bool Overlaps(Rect rectangle)
        {
            if (!UpdateEdgeControl())
                return false;

            return EdgeControl.Overlaps(this.ChangeCoordinatesTo(EdgeControl, rectangle));
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            Profiler.BeginSample("Edge.ContainsPoint");

            var result = UpdateEdgeControl() &&
                EdgeControl.ContainsPoint(this.ChangeCoordinatesTo(EdgeControl, localPoint));

            Profiler.EndSample();

            return result;
        }

        public virtual void OnPortChanged()
        {
            EdgeControl.outputOrientation = EdgeModel.FromPort?.Orientation ?? (EdgeModel.ToPort?.Orientation ?? Orientation.Horizontal);
            EdgeControl.inputOrientation = EdgeModel.ToPort?.Orientation ?? (EdgeModel.FromPort?.Orientation ?? Orientation.Horizontal);
            UpdateEdgeControl();
        }

        public virtual bool UpdateEdgeControl()
        {
            EdgeControl.UpdateLayout();
            EnableInClassList(k_EditModeClassName, EdgeModel.EditMode);
            UpdateEdgeControlColorsAndWidth();
            EdgeControl.MarkDirtyRepaint();
            return true;
        }

        protected void UpdateEdgeControlColorsAndWidth()
        {
            if (selected)
            {
                if (IsGhostEdge)
                    Debug.Log("Selected Ghost Edge: this should never be");

                EdgeControl.inputColor = SelectedColor;
                EdgeControl.outputColor = SelectedColor;
                EdgeControl.edgeWidth = EdgeWidth;
            }
            else
            {
                if (EdgeModel.ToPort != null)
                    EdgeControl.inputColor = Input.GetUI<Port>(GraphView)?.PortColor ?? Color.white;
                else if (EdgeModel.FromPort != null)
                    EdgeControl.inputColor = Output.GetUI<Port>(GraphView)?.PortColor ?? Color.white;

                if (EdgeModel.FromPort != null)
                    EdgeControl.outputColor = Output.GetUI<Port>(GraphView)?.PortColor ?? Color.white;
                else if (EdgeModel.ToPort != null)
                    EdgeControl.outputColor = Input.GetUI<Port>(GraphView)?.PortColor ?? Color.white;

                EdgeControl.edgeWidth = EdgeWidth;

                if (IsGhostEdge)
                {
                    EdgeControl.inputColor = new Color(EdgeControl.inputColor.r, EdgeControl.inputColor.g, EdgeControl.inputColor.b, 0.5f);
                    EdgeControl.outputColor = new Color(EdgeControl.outputColor.r, EdgeControl.outputColor.g, EdgeControl.outputColor.b, 0.5f);
                }
            }
        }

        protected override void OnCustomStyleResolved(ICustomStyle styles)
        {
            base.OnCustomStyleResolved(styles);

            if (styles.TryGetValue(s_EdgeWidthProperty, out var edgeWidthValue))
                EdgeWidth = edgeWidthValue;

            if (styles.TryGetValue(s_SelectedEdgeColorProperty, out var selectColorValue))
                SelectedColor = selectColorValue;

            if (styles.TryGetValue(s_EdgeColorProperty, out var edgeColorValue))
                DefaultColor = edgeColorValue;

            UpdateEdgeControlColorsAndWidth();
        }

        public override void OnSelected()
        {
            EdgeControl.RebuildControlPointsUI();
            EnableInClassList(k_EditModeClassName, EdgeModel.EditMode);
            UpdateEdgeControlColorsAndWidth();
        }

        public override void OnUnselected()
        {
            EnableInClassList(k_EditModeClassName, EdgeModel.EditMode);
            UpdateEdgeControlColorsAndWidth();
        }

        void OnEdgeAttach(AttachToPanelEvent e)
        {
            UpdateEdgeControl();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateEdgeControl();
        }

        public void UpdatePinning()
        {
        }

        public bool IsMovable => true;
    }
}