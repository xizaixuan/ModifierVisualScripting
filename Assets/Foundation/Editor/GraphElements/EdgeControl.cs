﻿using System;
using System.Collections.Generic;
using Unity.Modifier.GraphToolsFoundations.Bridge;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class EdgeControl : VisualElement
    {
        struct BezierSegment
        {
            // P0 is previous segment last point.
            public Vector2 p1;
            public Vector2 p2;
            public Vector2 p3;
        }

        VisualElement m_ControlPointContainer;
        List<BezierSegment> m_BezierSegments = new List<BezierSegment>();
        List<int> m_LineSegmentIndex = new List<int>();

        const int k_IntersectionSquaredRadius = 10000;
        const float k_ContainsPointDistance = 25f;

        public Vector2 BubblePosition
        {
            get
            {
                if (RenderPoints.Count > 0)
                {
                    // Find the segment that intersect a circle of radius sqrt(targetSqDistance) centered at from.
                    float targetSqDistance = Mathf.Min(k_IntersectionSquaredRadius, (to - from).sqrMagnitude / 4);
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
            RegisterCallback<DetachFromPanelEvent>(OnLeavePanel);

            pickingMode = PickingMode.Ignore;

            generateVisualContent += OnGenerateVisualContent;

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

        static float SquaredDistanceToSegment(Vector2 p, Vector2 s0, Vector2 s1)
        {
            var x = p.x;
            var y = p.y;
            var x1 = s0.x;
            var y1 = s0.y;
            var x2 = s1.x;
            var y2 = s1.y;

            var a = x - x1;
            var b = y - y1;
            var c = x2 - x1;
            var d = y2 - y1;

            var dot = a * c + b * d;
            var lenSq = c * c + d * d;
            float param = -1;
            if (lenSq > 0.000001f) //in case of 0 length line
                param = dot / lenSq;

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
                xx = x1 + param * c;
                yy = y1 + param * d;
            }

            var dx = x - xx;
            var dy = y - yy;
            return dx * dx + dy * dy;
        }

        public void RebuildControlPointsUI()
        {
            var edgeModel = (parent as Edge)?.EdgeModel;

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

        Mesh m_Mesh;

        Orientation m_InputOrientation;

        public Orientation inputOrientation
        {
            get { return m_InputOrientation; }
            set
            {
                if (m_InputOrientation == value)
                    return;
                m_InputOrientation = value;
                MarkDirtyRepaint();
            }
        }

        Orientation m_OutputOrientation;

        public Orientation outputOrientation
        {
            get => m_OutputOrientation;
            set => m_OutputOrientation = value;
        }

        Color m_InputColor = Color.grey;

        public Color inputColor
        {
            get => m_InputColor;
            set
            {
                if (m_InputColor != value)
                {
                    m_InputColor = value;
                    MarkDirtyRepaint();
                }
            }
        }

        Color m_OutputColor = Color.grey;

        public Color outputColor
        {
            get => m_OutputColor;
            set
            {
                if (m_OutputColor != value)
                {
                    m_OutputColor = value;
                    MarkDirtyRepaint();
                }
            }
        }

        int m_EdgeWidth = 2;

        public int edgeWidth
        {
            get { return m_EdgeWidth; }
            set
            {
                if (m_EdgeWidth == value)
                    return;
                m_EdgeWidth = value;
                UpdateLayout(); // The layout depends on the edges width
                MarkDirtyRepaint();
            }
        }

        float m_InterceptWidth = 5;

        public float interceptWidth
        {
            get { return m_InterceptWidth; }
            set { m_InterceptWidth = value; }
        }

        // The start of the edge in graph coordinates.
        public Vector2 from => (parent as Edge)?.From ?? Vector2.zero;

        // The end of the edge in graph coordinates.
        public Vector2 to => (parent as Edge)?.To ?? Vector2.zero;

        public Vector2 ControlPointOffset { get; set; }

        // The control points in graph coordinates.
        Vector2[] m_ControlPoints;

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Profiler.BeginSample("DrawEdge");
            DrawEdge(mgc);
            Profiler.EndSample();
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            FindNearestCurveSegment(localPoint, out var minDistance, out _, out _);
            return minDistance < k_ContainsPointDistance;
        }

        public override bool Overlaps(Rect rect)
        {
            if (base.Overlaps(rect))
            {
                for (int a = 0; a < m_RenderPoints.Count - 1; a++)
                {
                    if (RectUtils.IntersectsSegment(rect, m_RenderPoints[a], m_RenderPoints[a + 1]))
                        return true;
                }
            }

            return false;
        }

        public void PointsChanged()
        {
            MarkDirtyRepaint();
        }

        // The points that will be rendered. Expressed in coordinates local to the element.
        List<Vector2> m_RenderPoints = new List<Vector2>();
        protected List<Vector2> RenderPoints => m_RenderPoints;

        public void UpdateLayout()
        {
            if (parent == null) return;
            ComputeLayout();
            MarkDirtyRepaint();
        }

        protected virtual void UpdateRenderPoints()
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

                Vector2 p1 = parent.ChangeCoordinatesTo(this, bezierSegment.p1);
                Vector2 p2 = parent.ChangeCoordinatesTo(this, bezierSegment.p2);
                p3 = parent.ChangeCoordinatesTo(this, bezierSegment.p3);

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

        void ComputeLayout()
        {
            ComputeCurveSegmentsFromControlPoints();

            // Compute VisualElement position and dimension.
            var edgeModel = (parent as Edge)?.EdgeModel;

            if (edgeModel == null)
            {
                style.top = 0;
                style.left = 0;
                style.width = 0;
                style.height = 0;
                return;
            }

            Rect rect = new Rect(from, Vector2.zero);
            foreach (var bezierSegment in m_BezierSegments)
            {
                var pt = bezierSegment.p1;
                rect.xMin = Math.Min(rect.xMin, pt.x);
                rect.yMin = Math.Min(rect.yMin, pt.y);
                rect.xMax = Math.Max(rect.xMax, pt.x);
                rect.yMax = Math.Max(rect.yMax, pt.y);

                pt = bezierSegment.p2;
                rect.xMin = Math.Min(rect.xMin, pt.x);
                rect.yMin = Math.Min(rect.yMin, pt.y);
                rect.xMax = Math.Max(rect.xMax, pt.x);
                rect.yMax = Math.Max(rect.yMax, pt.y);

                pt = bezierSegment.p3;
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

            var edgeModel = (parent as Edge)?.EdgeModel;
            var graphView = GetFirstAncestorOfType<GraphView>();

            if (graphView == null)
                return;

            var fromOrientation = edge.Output?.Orientation ?? edge.Input?.Orientation ?? Orientation.Horizontal;
            var toOrientation = edge.Input?.Orientation ?? fromOrientation;

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
                    p1 = previous + directionFrom * (length * previousTightness),
                    p2 = localSplitPoint + directionTo * (length * tightness),
                    p3 = localSplitPoint,
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
                p1 = previous + directionFrom * (length * previousTightness),
                p2 = to + directionTo * length,
                p3 = to,
            });

            // Update VisualElement positions for control point
            for (var i = 0; i < m_BezierSegments.Count - 1; i++)
            {
                if (i >= m_ControlPointContainer.childCount)
                    break;

                (m_ControlPointContainer[i] as EdgeControlPoint)?.SetPositions(m_BezierSegments[i].p3, m_BezierSegments[i].p2, m_BezierSegments[i + 1].p1);
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

        void DrawEdge(MeshGenerationContext mgc)
        {
            if (edgeWidth <= 0)
                return;

            UpdateRenderPoints();
            if (m_RenderPoints.Count == 0)
                return; // Don't draw anything

            Color inColor = inputColor;
            Color outColor = outputColor;

#if UNITY_EDITOR
            inColor *= GraphViewStaticBridge.EditorPlayModeTint;
            outColor *= GraphViewStaticBridge.EditorPlayModeTint;
#endif // UNITY_EDITOR

            uint cpt = (uint)m_RenderPoints.Count;
            uint wantedLength = (cpt) * 2;
            uint indexCount = (wantedLength - 2) * 3;

            var md = GraphViewStaticBridge.AllocateMeshWriteData(mgc, (int)wantedLength, (int)indexCount);
            if (md.vertexCount == 0)
                return;

            float polyLineLength = 0;
            for (int i = 1; i < cpt; ++i)
                polyLineLength += (m_RenderPoints[i - 1] - m_RenderPoints[i]).sqrMagnitude;

            float halfWidth = edgeWidth * 0.5f;
            float currentLength = 0;

            Vector2 unitPreviousSegment = Vector2.zero;
            for (int i = 0; i < cpt; ++i)
            {
                Vector2 dir;
                Vector2 unitNextSegment = Vector2.zero;
                Vector2 nextSegment = Vector2.zero;

                if (i < cpt - 1)
                {
                    nextSegment = (m_RenderPoints[i + 1] - m_RenderPoints[i]);
                    unitNextSegment = nextSegment.normalized;
                }


                if (i > 0 && i < cpt - 1)
                {
                    dir = unitPreviousSegment + unitNextSegment;
                    dir.Normalize();
                }
                else if (i > 0)
                {
                    dir = unitPreviousSegment;
                }
                else
                {
                    dir = unitNextSegment;
                }

                Vector2 pos = m_RenderPoints[i];
                Vector2 uv = new Vector2(dir.y * halfWidth, -dir.x * halfWidth); // Normal scaled by half width
                Color32 tint = Color.LerpUnclamped(outColor, inColor, currentLength / polyLineLength);

                md.SetNextVertex(new Vector3(pos.x, pos.y, 1), uv, tint);
                md.SetNextVertex(new Vector3(pos.x, pos.y, -1), uv, tint);

                if (i < cpt - 2)
                {
                    currentLength += nextSegment.sqrMagnitude;
                }
                else
                {
                    currentLength = polyLineLength;
                }

                unitPreviousSegment = unitNextSegment;
            }

            // Fill triangle indices as it is a triangle strip
            for (uint i = 0; i < wantedLength - 2; ++i)
            {
                if ((i & 0x01) == 0)
                {
                    md.SetNextIndex((UInt16)i);
                    md.SetNextIndex((UInt16)(i + 2));
                    md.SetNextIndex((UInt16)(i + 1));
                }
                else
                {
                    md.SetNextIndex((UInt16)i);
                    md.SetNextIndex((UInt16)(i + 1));
                    md.SetNextIndex((UInt16)(i + 2));
                }
            }
        }

        void OnLeavePanel(DetachFromPanelEvent e)
        {
            if (m_Mesh != null)
            {
                UnityEngine.Object.DestroyImmediate(m_Mesh);
                m_Mesh = null;
            }
        }
    }
}