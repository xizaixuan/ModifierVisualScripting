﻿using System;
using System.Collections.Generic;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Unity.Modifier.GraphElements
{
    public class CreateEdgeAction : IAction
    {
        [Flags]
        public enum PortAlignmentType
        {
            None = 0,
            Input = 1,
            Output = 2,
        }

        public readonly IGTFPortModel InputPortModel;
        public readonly IGTFPortModel OutputPortModel;
        public readonly IEnumerable<IGTFEdgeModel> EdgeModelsToDelete;
        public readonly PortAlignmentType PortAlignment;

        public CreateEdgeAction(IGTFPortModel inputPortModel, IGTFPortModel outputPortModel,
                                IEnumerable<IGTFEdgeModel> edgeModelsToDelete = null, PortAlignmentType portAlignment = PortAlignmentType.None)
        {
            Assert.IsTrue(inputPortModel.Direction == Direction.Input);
            Assert.IsTrue(outputPortModel.Direction == Direction.Output);
            InputPortModel = inputPortModel;
            OutputPortModel = outputPortModel;
            EdgeModelsToDelete = edgeModelsToDelete;
            PortAlignment = portAlignment;
        }

        public static TState DefaultReducer<TState>(TState previousState, CreateEdgeAction action) where TState : State
        {
            var graphModel = previousState.GraphModel;

            if (action.EdgeModelsToDelete != null)
                graphModel.DeleteElements(action.EdgeModelsToDelete);

            IGTFPortModel outputPortModel = action.OutputPortModel;
            IGTFPortModel inputPortModel = action.InputPortModel;

            graphModel.CreateEdgeGTF(inputPortModel, outputPortModel);

            return previousState;
        }
    }

    public class AddControlPointOnEdgeAction : IAction
    {
        public readonly IGTFEdgeModel EdgeModel;
        public readonly int AtIndex;
        public readonly Vector2 Position;

        public AddControlPointOnEdgeAction(IGTFEdgeModel edgeModel, int atIndex, Vector2 position)
        {
            EdgeModel = edgeModel;
            AtIndex = atIndex;
            Position = position;
        }

        public static TState DefaultReducer<TState>(TState previousState, AddControlPointOnEdgeAction action) where TState : State
        {
            var graphModel = previousState.AssetModel;
            Undo.RegisterCompleteObjectUndo(graphModel as Object, "Insert Control Point");
            action.EdgeModel.InsertEdgeControlPoint(action.AtIndex, action.Position, 100);
            previousState.MarkForUpdate(UpdateFlags.UpdateView, action.EdgeModel);
            return previousState;
        }
    }

    public class MoveEdgeControlPointAction : IAction
    {
        public readonly IGTFEdgeModel EdgeModel;
        public readonly int EdgeIndex;
        public readonly Vector2 NewPosition;
        public readonly float NewTightness;

        public MoveEdgeControlPointAction(IGTFEdgeModel edgeModel, int edgeIndex, Vector2 newPosition, float newTightness)
        {
            EdgeModel = edgeModel;
            EdgeIndex = edgeIndex;
            NewPosition = newPosition;
            NewTightness = newTightness;
        }

        public static TState DefaultReducer<TState>(TState previousState, MoveEdgeControlPointAction action) where TState : State
        {
            var graphModel = previousState.AssetModel;
            Undo.RegisterCompleteObjectUndo(graphModel as Object, "Edit Control Point");
            action.EdgeModel.ModifyEdgeControlPoint(action.EdgeIndex, action.NewPosition, action.NewTightness);
            return previousState;
        }
    }

    public class RemoveEdgeControlPointAction : IAction
    {
        public readonly IGTFEdgeModel EdgeModel;
        public readonly int EdgeIndex;

        public RemoveEdgeControlPointAction(IGTFEdgeModel edgeModel, int edgeIndex)
        {
            EdgeModel = edgeModel;
            EdgeIndex = edgeIndex;
        }

        public static TState DefaultReducer<TState>(TState previousState, RemoveEdgeControlPointAction action) where TState : State
        {
            var graphModel = previousState.AssetModel;
            Undo.RegisterCompleteObjectUndo(graphModel as Object, "Remove Control Point");
            action.EdgeModel.RemoveEdgeControlPoint(action.EdgeIndex);
            previousState.MarkForUpdate(UpdateFlags.UpdateView, action.EdgeModel);
            return previousState;
        }
    }

    public class SetEdgeEditModeAction : IAction
    {
        public readonly IGTFEdgeModel EdgeModel;
        public readonly bool Value;

        public SetEdgeEditModeAction(IGTFEdgeModel edgeModel, bool value)
        {
            EdgeModel = edgeModel;
            Value = value;
        }

        public static TState DefaultReducer<TState>(TState previousState, SetEdgeEditModeAction action) where TState : State
        {
            action.EdgeModel.EditMode = action.Value;
            previousState.MarkForUpdate(UpdateFlags.UpdateView, action.EdgeModel);
            return previousState;
        }
    }
}