﻿using System;
using System.Collections.Generic;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class EdgeConnectorListener
    {
        Action<IStore, Edge, Vector2> m_OnDropOutsideDelegate;
        Action<IStore, Edge> m_OnDropDelegate;

        public void SetDropOutsideDelegate(Action<IStore, Edge, Vector2> action)
        {
            m_OnDropOutsideDelegate = action;
        }

        public void SetDropDelegate(Action<IStore, Edge> action)
        {
            m_OnDropDelegate = action;
        }

        public static List<IGTFEdgeModel> GetDropEdgeModelsToDelete(IGTFEdgeModel edge)
        {
            List<IGTFEdgeModel> edgeModelsToDelete = new List<IGTFEdgeModel>();

            if (edge.ToPort != null && edge.ToPort.Capacity == PortCapacity.Single)
            {
                foreach (var edgeToDelete in edge.ToPort.ConnectedEdges)
                {
                    if (!ReferenceEquals(edgeToDelete, edge))
                        edgeModelsToDelete.Add(edgeToDelete);
                }
            }

            if (edge.FromPort != null && edge.FromPort.Capacity == PortCapacity.Single)
            {
                foreach (var edgeToDelete in edge.FromPort.ConnectedEdges)
                {
                    if (!ReferenceEquals(edgeToDelete, edge))
                        edgeModelsToDelete.Add(edgeToDelete);
                }
            }

            return edgeModelsToDelete;
        }

        public void OnDropOutsidePort(IStore store, Edge edge, Vector2 position, Edge originalEdge)
        {
            if (m_OnDropOutsideDelegate != null)
            {
                m_OnDropOutsideDelegate(store, edge, position);
            }
            else
            {
                GraphView graphView = edge.GetFirstAncestorOfType<GraphView>();
                position = graphView.contentViewContainer.WorldToLocal(position);

                List<IGTFEdgeModel> edgesToDelete = GetDropEdgeModelsToDelete(edge.EdgeModel);

                // when grabbing an existing edge's end, the edgeModel should be deleted
                if (!(edge.EdgeModel is GhostEdgeModel))
                    edgesToDelete.Add(edge.EdgeModel);

                IGTFPortModel existingPortModel;

                // warning: when dragging the end of an existing edge, both ports are non null.
                if (edge.Input != null && edge.Output != null)
                {
                    float distanceToOutput = Vector2.Distance(edge.EdgeControl.from, position);
                    float distanceToInput = Vector2.Distance(edge.EdgeControl.to, position);

                    // note: if the user was able to stack perfectly both ports, we'd be in trouble
                    if (distanceToOutput < distanceToInput)
                        existingPortModel = edge.Input;
                    else
                        existingPortModel = edge.Output;
                }
                else
                {
                    existingPortModel = edge.Input ?? edge.Output;
                }

                if (originalEdge != null)
                    edgesToDelete.Add(originalEdge.EdgeModel);

                store.Dispatch(new DropEdgeInEmptyRegionAction(existingPortModel, position, edgesToDelete));
            }
        }

        public void OnDrop(IStore store, Edge edge, Edge originalEdge)
        {
            if (m_OnDropDelegate != null)
            {
                m_OnDropDelegate(store, edge);
            }
            else
            {
                List<IGTFEdgeModel> edgeModelsToDelete = GetDropEdgeModelsToDelete(edge.EdgeModel);

                if (edge.EdgeModel.ToPort.IsConnectedTo(edge.EdgeModel.FromPort))
                    return;

                // when grabbing an existing edge's end, the edgeModel should be deleted
                if (!(edge.EdgeModel is GhostEdgeModel))
                    edgeModelsToDelete.Add(edge.EdgeModel);

                if (originalEdge != null)
                    edgeModelsToDelete.Add(originalEdge.EdgeModel);

                store.Dispatch(new CreateEdgeAction(
                    edge.EdgeModel.ToPort,
                    edge.EdgeModel.FromPort,
                    edgeModelsToDelete
                ));
            }
        }
    }
}