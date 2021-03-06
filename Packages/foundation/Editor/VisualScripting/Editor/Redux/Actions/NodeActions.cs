﻿using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class CreateLogNodeAction : IAction
    {
        public readonly IStackModel StackModel;
        public readonly LogNodeModel.LogTypes LogType;

        public CreateLogNodeAction(IStackModel stackModel, LogNodeModel.LogTypes logType)
        {
            StackModel = stackModel;
            LogType = logType;
        }
    }

    public class DisconnectNodeAction : IAction
    {
        public readonly INodeModel[] NodeModels;

        public DisconnectNodeAction(params INodeModel[] nodeModels)
        {
            NodeModels = nodeModels;
        }
    }

    public class RemoveNodesAction : IAction
    {
        public readonly INodeModel[] ElementsToRemove;
        public readonly INodeModel[] NodesToBypass;

        public RemoveNodesAction(INodeModel[] nodesToBypass, INodeModel[] elementsToRemove)
        {
            ElementsToRemove = elementsToRemove;
            NodesToBypass = nodesToBypass;
        }
    }

    public class CreateNodeFromSearcherAction : IAction
    {
        public readonly IGraphModel GraphModel;
        public readonly Vector2 Position;
        public readonly GraphNodeModelSearcherItem SelectedItem;
        public readonly IReadOnlyList<GUID> Guids;

        public CreateNodeFromSearcherAction(IGraphModel graphModel, Vector2 position,
                                            GraphNodeModelSearcherItem selectedItem, IReadOnlyList<GUID> guids = null)
        {
            GraphModel = graphModel;
            Position = position;
            SelectedItem = selectedItem;
            Guids = guids;
        }
    }

    public class SetNodeEnabledStateAction : IAction
    {
        public readonly INodeModel[] NodeToConvert;
        public readonly ModelState State;

        public SetNodeEnabledStateAction(INodeModel[] nodeModel, ModelState state)
        {
            State = state;
            NodeToConvert = nodeModel;
        }
    }
}