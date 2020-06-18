using System;
using System.Collections.Generic;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Modifier.VisualScripting.Editor.Edge;
using IEdgeModel = UnityEditor.Modifier.VisualScripting.GraphViewModel.IEdgeModel;

namespace Modifier.VisualScripting.Editor.Elements
{
    public class EdgeControlPoint : VisualElement
    {
        static readonly string k_ClassName = "edge-control-point";

        EdgeControl m_EdgeControl;
        IEdgeModel m_EdgeModel;
        int m_ControlPointIndex;

        public EdgeControlPoint(EdgeControl edgeControl, IEdgeModel edgeModel, int controlPointIndex)
        {
            m_EdgeControl = edgeControl;
            m_EdgeModel = edgeModel;
            m_ControlPointIndex = controlPointIndex;

            AddToClassList(k_ClassName);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);

            style.position = Position.Absolute;
        }

        bool m_DraggingControlPoint;
        bool m_DraggingTightness;
        Vector2 m_OriginalElementPosition;
        float m_OriginalTightness;
        Vector2 m_OriginalPointerPosition;

        void OnPointerDown(PointerDownEvent e)
        {
            if (!e.isPrimary || e.button != 0)
                return;

            m_OriginalPointerPosition = this.ChangeCoordinatesTo(parent, e.localPosition);
            m_OriginalElementPosition = m_EdgeModel.EdgeControlPoints[m_ControlPointIndex].Position;
            m_OriginalTightness = m_EdgeModel.EdgeControlPoints[m_ControlPointIndex].Tightness;

            if (e.modifiers == EventModifiers.None)
            {
                m_DraggingControlPoint = true;
            }
            else if (e.modifiers == EventModifiers.Alt)
            {
                m_DraggingTightness = true;
            }

            if (m_DraggingControlPoint || m_DraggingTightness)
            {
                this.CapturePointer(e.pointerId);
                e.StopPropagation();
            }
        }

        void OnPointerMove(PointerMoveEvent e)
        {
            VseGraphView graphView = null;
            Vector2 pointerDelta = Vector2.zero;
            if (m_DraggingControlPoint || m_DraggingTightness)
            {
                graphView = GetFirstAncestorOfType<VseGraphView>();
                var pointerPosition = this.ChangeCoordinatesTo(parent, e.localPosition);
                pointerDelta = new Vector2(pointerPosition.x, pointerPosition.y) - m_OriginalPointerPosition;
            }

            if (graphView == null)
            {
                return;
            }

            if (m_DraggingControlPoint)
            {
                var newPosition = m_OriginalElementPosition + pointerDelta;
                graphView.store.Dispatch(new MoveEdgeControlPointAction(m_EdgeModel, m_ControlPointIndex, newPosition, m_OriginalTightness));
                m_EdgeControl.MarkDirtyRepaint();
                e.StopPropagation();
            }
            else if (m_DraggingTightness)
            {
                var tightnessDelta = pointerDelta.x - pointerDelta.y;
                var newTightness = m_OriginalTightness + tightnessDelta;
                graphView.store.Dispatch(new MoveEdgeControlPointAction(m_EdgeModel, m_ControlPointIndex, m_OriginalElementPosition, newTightness));
                e.StopPropagation();
                m_EdgeControl.MarkDirtyRepaint();
            }
        }

        void OnPointerUp(PointerUpEvent e)
        {
            if (!e.isPrimary || e.button != 0)
                return;

            this.ReleasePointer(e.pointerId);
            m_DraggingControlPoint = false;
            m_DraggingTightness = false;
            e.StopPropagation();
        }

        public void SetPositions(Vector2 cpPosition, Vector2 lhPosition, Vector2 rhPosition)
        {
            style.left = cpPosition.x;
            style.top = cpPosition.y;
        }
    }

    public class EdgeControl : Unity.Modifier.GraphElements.EdgeControl
    {
        struct BezierSegment
        {
            // P0 is previous segment last point.
            public Vector2 m_P1;
            public Vector2 m_P2;
            public Vector2 m_P3;
        }

        VisualElement m_ControlPointContainer;
        List<BezierSegment> m_BezierSegments = new List<BezierSegment>();
        List<int> m_LineSegmentIndex = new List<int>();

        public Vector2 BubblePosition
        {
            get
            {
                if (RenderPoints.Count > 0)
                {
                    // Find the segment that intersect a circle of radius sqrt(targetSqDistance) centered at from.
                    float targetSqDistance = Mathf.Min(10000, (to - from).sqrMagnitude / 4);
                    var localFrom = parent.ChangeCoordinatesTo(this, from);
                    for (var index = 0; index < RenderPoints.Count; index++)
                    {
                        var point = RenderPoints[index];
                        if ((point - localFrom).sqrMagnitude >= targetSqDistance)
                        {
                            return this.ChangeCoordinatesTo(parent, RenderPoints[index]);
                        }
                    }
                }

                return this.ChangeCoordinatesTo(parent, Vector2.zero);
            }
        }

        public EdgeControl()
        {
            m_ControlPointContainer = new VisualElement { name = "control-points-container" };
            m_ControlPointContainer.style.position = Position.Absolute;
            pickingMode = PickingMode.Position;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent e)
        {
            parent.Add(m_ControlPointContainer);
        }

        void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            m_ControlPointContainer.RemoveFromHierarchy();
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            FindNearestCurveSegment(localPoint, out var minDistance, out _, out _);
            return minDistance < 25;
        }

        public void FindNearestCurveSegment(Vector2 localPoint, out float minSquareDistance, out int nearestControlPointIndex, out int nearestRenderPointIndex)
        {
            minSquareDistance = Single.MaxValue;
            nearestRenderPointIndex = Int32.MaxValue;
            for (var index = 0; index < RenderPoints.Count - 1; index++)
            {
                var a = RenderPoints[index];
                var b = RenderPoints[index + 1];
                var squareDistance = SquaredDistanceToSegment(localPoint, a, b);
                if (squareDistance < minSquareDistance)
                {
                    minSquareDistance = squareDistance;
                    nearestRenderPointIndex = index;
                }
            }

            nearestControlPointIndex = 0;
            while (nearestControlPointIndex < m_LineSegmentIndex.Count && nearestRenderPointIndex >= m_LineSegmentIndex[nearestControlPointIndex])
            {
                nearestControlPointIndex++;
            }

            nearestControlPointIndex--;
        }

        static float SquaredDistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var x = p.x;
            var y = p.y;
            var x1 = a.x;
            var y1 = a.y;
            var x2 = b.x;
            var y2 = b.y;

            var A = x - x1;
            var B = y - y1;
            var C = x2 - x1;
            var D = y2 - y1;

            var dot = A * C + B * D;
            var len_sq = C * C + D * D;
            float param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            float xx, yy;

            if (param < 0)
            {
                xx = x1;
                yy = y1;
            }
            else if (param > 1)
            {
                xx = x2;
                yy = y2;
            }
            else
            {
                xx = x1 + param * C;
                yy = y1 + param * D;
            }

            var dx = x - xx;
            var dy = y - yy;
            return dx * dx + dy * dy;
        }

        public void RebuildControlPointsUI()
        {
            var edgeModel = (parent as Edge)?.model;

            if (edgeModel == null)
                return;

            while (m_ControlPointContainer.childCount > edgeModel.EdgeControlPoints.Count)
            {
                m_ControlPointContainer.RemoveAt(m_ControlPointContainer.childCount - 1);
            }

            while (m_ControlPointContainer.childCount < edgeModel.EdgeControlPoints.Count)
            {
                var cp = new EdgeControlPoint(this, edgeModel, m_ControlPointContainer.childCount);
                m_ControlPointContainer.Add(cp);
            }

            UpdateLayout();
        }

        public override void UpdateLayout()
        {
            if (parent == null) return;
            ComputeLayout(); // Update the element layout based on the control points.
            MarkDirtyRepaint();
        }

        void ComputeLayout()
        {
            ComputeCurveSegmentsFromControlPoints();

            // Compute VisualElement position and dimension.
            var edgeModel = (parent as Edge)?.model;

            if (edgeModel == null)
            {
                style.top = 0;
                style.left = 0;
                style.width = 0;
                style.height = 0;
                return;
            }

            Rect rect = new Rect(from, Vector2.zero);
            for (var i = 0; i < m_BezierSegments.Count; ++i)
            {
                var pt = m_BezierSegments[i].m_P1;
                rect.xMin = Math.Min(rect.xMin, pt.x);
                rect.yMin = Math.Min(rect.yMin, pt.y);
                rect.xMax = Math.Max(rect.xMax, pt.x);
                rect.yMax = Math.Max(rect.yMax, pt.y);

                pt = m_BezierSegments[i].m_P2;
                rect.xMin = Math.Min(rect.xMin, pt.x);
                rect.yMin = Math.Min(rect.yMin, pt.y);
                rect.xMax = Math.Max(rect.xMax, pt.x);
                rect.yMax = Math.Max(rect.yMax, pt.y);

                pt = m_BezierSegments[i].m_P3;
                rect.xMin = Math.Min(rect.xMin, pt.x);
                rect.yMin = Math.Min(rect.yMin, pt.y);
                rect.xMax = Math.Max(rect.xMax, pt.x);
                rect.yMax = Math.Max(rect.yMax, pt.y);
            }

            var p = rect.position;
            var dim = rect.size;
            style.left = p.x;
            style.top = p.y;
            style.width = dim.x;
            style.height = dim.y;
        }

        void ComputeCurveSegmentsFromControlPoints()
        {
            var edge = parent as Edge;

            if (edge == null)
                return;

            var edgeModel = (parent as Edge)?.model;
            var graphView = GetFirstAncestorOfType<GraphView>();

            if (graphView == null)
                return;

            var fromOrientation = edge.output?.orientation ?? edge.input?.orientation ?? Orientation.Horizontal;
            var toOrientation = edge.input?.orientation ?? fromOrientation;

            m_BezierSegments.Clear();

            var previous = from;
            var previousTightness = 1f;
            var directionFrom = fromOrientation == Orientation.Horizontal ? Vector2.right : Vector2.up;
            Vector2 directionTo;
            float length;
            for (var i = 0; i < edgeModel?.EdgeControlPoints.Count; i++)
            {
                var tightness = edgeModel.EdgeControlPoints[i].Tightness / 100;

                var splitPoint = edgeModel.EdgeControlPoints[i].Position;
                splitPoint += ControlPointOffset;
                var localSplitPoint = graphView.contentViewContainer.ChangeCoordinatesTo(parent, splitPoint);
                length = ControlPointDistance(previous, localSplitPoint, fromOrientation);

                Vector2 next;
                if (i == edgeModel.EdgeControlPoints.Count - 1)
                {
                    next = to;
                }
                else
                {
                    next = edgeModel.EdgeControlPoints[i + 1].Position;
                    next += ControlPointOffset;
                    next = graphView.contentViewContainer.ChangeCoordinatesTo(parent, next);
                }
                directionTo = (previous - next).normalized;

                var segment = new BezierSegment()
                {
                    m_P1 = previous + directionFrom * (length * previousTightness),
                    m_P2 = localSplitPoint + directionTo * (length * tightness),
                    m_P3 = localSplitPoint,
                };
                m_BezierSegments.Add(segment);

                previous = localSplitPoint;
                previousTightness = tightness;
                directionFrom = -directionTo;
            }

            length = ControlPointDistance(previous, to, fromOrientation);
            directionTo = toOrientation == Orientation.Horizontal ? Vector2.left : Vector2.down;

            m_BezierSegments.Add(new BezierSegment()
            {
                m_P1 = previous + directionFrom * (length * previousTightness),
                m_P2 = to + directionTo * length,
                m_P3 = to,
            });

            // Update VisualElement positions for control point
            for (var i = 0; i < m_BezierSegments.Count - 1; i++)
            {
                if (i >= m_ControlPointContainer.childCount)
                    break;

                (m_ControlPointContainer[i] as EdgeControlPoint)?.SetPositions(m_BezierSegments[i].m_P3, m_BezierSegments[i].m_P2, m_BezierSegments[i + 1].m_P1);
            }
        }

        // Compute the distance of Bezier curve control points P1 and P2 from P0 and P3 respectively.
        static float ControlPointDistance(Vector2 from, Vector2 to, Orientation orientation)
        {
            float xd, yd;
            if (orientation == Orientation.Horizontal)
            {
                xd = to.x - @from.x;
                yd = Mathf.Abs(to.y - @from.y);
            }
            else
            {
                xd = to.y - @from.y;
                yd = Mathf.Abs(to.x - @from.x);
            }

            // Max length is half the x distance.
            // When x distance is small or negative, we use a value based on the y distance mapped to [100, 250]
            var yCorr = 100f + Mathf.Min(150f, yd * .8f);
            float maxLength = Mathf.Max(xd, yCorr) * .5f;

            // When distance is small, we want the control points P1 and P2 to be near P0 and P3.
            // When distance is large, we want the control points P1 and P2 to be at maxLength from P0 and P3.
            var d = Mathf.Max(Mathf.Abs(xd), yd) * 0.01f;
            d *= d;
            var factor = d / (1f + d);

            return factor * maxLength;
        }

        protected override void UpdateRenderPoints()
        {
            // TODO
            // Dirty system

            ComputeLayout();

            RenderPoints.Clear();
            m_LineSegmentIndex.Clear();

            Vector2 p0 = parent.ChangeCoordinatesTo(this, from);
            Vector2 p3 = parent.ChangeCoordinatesTo(this, to);
            for (var index = 0; index < m_BezierSegments.Count; index++)
            {
                var bezierSegment = m_BezierSegments[index];
                m_LineSegmentIndex.Add(RenderPoints.Count);

                Vector2 p1 = parent.ChangeCoordinatesTo(this, bezierSegment.m_P1);
                Vector2 p2 = parent.ChangeCoordinatesTo(this, bezierSegment.m_P2);
                p3 = parent.ChangeCoordinatesTo(this, bezierSegment.m_P3);

                int deepness = 0;
                GenerateRenderPoints(p0, p1, p2, p3, deepness);

                p0 = p3;
            }

            RenderPoints.Add(p3);
            m_LineSegmentIndex.Add(RenderPoints.Count);
        }

        static bool StraightEnough(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // This computes an upper bound on the distance between the Bezier curve
            // and a straight line going from p0 to p3.
            // See https://hcklbrrfnn.files.wordpress.com/2012/08/bez.pdf
            // Summary: - define a straight Bezier line L going from p0 and p3 in terms of p0, p1, p2 and p3
            //          - subtract both curves: B - L =  (1 − t)t ((1 − t) u + t v)
            //          - compute the magnitude of the difference: D = ||B - L||^2
            //          - compute an upper bound on the magnitude: 1/16 * (Max(ux^2, vx^2) + Max(uy^2, vy^2))
            var u = 3 * p1 - 2 * p0 - p3;
            var v = 3 * p2 - 2 * p3 - p0;
            u = Vector2.Max(u, v);

            // Return true if the curve does not deviate from a straight line by more than 1.
            return u.x * u.x + u.y * u.y < 0.0625f;
        }

        void GenerateRenderPoints(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int deepness)
        {
            if (StraightEnough(p0, p1, p2, p3) || deepness > 6)
            {
                RenderPoints.Add(p0);
                return;
            }

            // DeCasteljau algorithm.

            var midpoint = (p1 + p2) * 0.5f;
            var left1 = (p0 + p1) * 0.5f;
            var right2 = (p2 + p3) * 0.5f;

            var left2 = (left1 + midpoint) * 0.5f;
            var right1 = (right2 + midpoint) * 0.5f;

            var split = (left2 + right1) * 0.5f;

            GenerateRenderPoints(p0, left1, left2, split, deepness + 1);
            GenerateRenderPoints(split, right1, right2, p3, deepness + 1);
        }
    }
}