﻿using System;
using System.Reflection;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public static class VSCreationExtensions
    {
        public static StackBaseModel CreateStack(this GraphViewModel.IGraphModel graphModel, string name, Vector2 position,
            SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            var stackTypeToCreate = graphModel.Stencil.GetDefaultStackModelType();
            if (!typeof(StackModel).IsAssignableFrom(stackTypeToCreate))
                stackTypeToCreate = typeof(StackModel);

            return (StackBaseModel)graphModel.CreateNode(stackTypeToCreate, name, position, spawnFlags, guid: guid);
        }

        public static UnaryOperatorNodeModel CreateUnaryStatementNode(this IStackModel stackModel,
            UnaryOperatorKind kind, int index, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            return stackModel.CreateStackedNode<UnaryOperatorNodeModel>(kind.ToString(), index, spawnFlags, n => n.Kind = kind, guid);
        }

        public static UnaryOperatorNodeModel CreateUnaryOperatorNode(this GraphViewModel.IGraphModel graphModel,
            UnaryOperatorKind kind, Vector2 position, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            return graphModel.CreateNode<UnaryOperatorNodeModel>(kind.ToString(), position, spawnFlags, n => n.Kind = kind, guid);
        }

        public static BinaryOperatorNodeModel CreateBinaryOperatorNode(this GraphViewModel.IGraphModel graphModel,
            BinaryOperatorKind kind, Vector2 position, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            return graphModel.CreateNode<BinaryOperatorNodeModel>(kind.ToString(), position, spawnFlags, n => n.Kind = kind, guid);
        }

        public static IVariableModel CreateVariableNode(this GraphViewModel.IGraphModel graphModel,
            IVariableDeclarationModel declarationModel, Vector2 position, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            return graphModel.Stencil.CreateVariableModelForDeclaration(graphModel, declarationModel, position, spawnFlags, guid);
        }

        public static IConstantNodeModel CreateConstantNode(this GraphViewModel.IGraphModel graphModel, string constantName,
            TypeHandle constantTypeHandle, Vector2 position, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null, Action<ConstantNodeModel> preDefine = null)
        {
            var nodeType = graphModel.Stencil.GetConstantNodeModelType(constantTypeHandle);

            void PreDefineSetup(NodeModel model)
            {
                if (model is ConstantNodeModel constantModel)
                {
                    constantModel.PredefineSetup(constantTypeHandle);
                    preDefine?.Invoke(constantModel);
                }
            }

            return (ConstantNodeModel)graphModel.CreateNode(nodeType, constantName, position, spawnFlags, PreDefineSetup, guid);
        }

        public static SetPropertyGroupNodeModel CreateSetPropertyGroupNode(this IStackModel stackModel, int index, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            var nodeModel = stackModel.CreateStackedNode<SetPropertyGroupNodeModel>(SetPropertyGroupNodeModel.k_Title, index, spawnFlags, guid: guid);
            return nodeModel;
        }

        public static GetPropertyGroupNodeModel CreateGetPropertyGroupNode(this GraphViewModel.IGraphModel graphModel,
            Vector2 position, SpawnFlags spawnFlags = SpawnFlags.Default, GUID? guid = null)
        {
            return graphModel.CreateNode<GetPropertyGroupNodeModel>(GetPropertyGroupNodeModel.k_Title, position, spawnFlags, guid: guid);
        }
    }

    public static class VSNodeDataCreationExtensions
    {
        public static StackBaseModel CreateStack(this IGraphNodeCreationData data, string name)
        {
            return data.GraphModel.CreateStack(name, data.Position, data.SpawnFlags, data.Guid);
        }

        public static INodeModel CreateNode(this IGraphNodeCreationData data, Type nodeType, string name = null, Action<NodeModel> preDefineSetup = null)
        {
            return data.GraphModel.CreateNode(nodeType, name, data.Position, data.SpawnFlags, preDefineSetup, data.Guid);
        }

        public static INodeModel CreateNode(this IStackedNodeCreationData data, Type nodeType, string name = null, Action<NodeModel> preDefineSetup = null)
        {
            return data.StackModel.CreateStackedNode(nodeType, name, data.Index, data.SpawnFlags, preDefineSetup, data.Guid);
        }

        public static T CreateNode<T>(this IGraphNodeCreationData data, string name = null, Action<T> preDefineSetup = null) where T : NodeModel
        {
            return data.GraphModel.CreateNode(name, data.Position, data.SpawnFlags, preDefineSetup, data.Guid);
        }

        public static T CreateNode<T>(this IStackedNodeCreationData data, string name = null, Action<T> preDefineSetup = null) where T : NodeModel
        {
            return data.StackModel.CreateStackedNode(name, data.Index, data.SpawnFlags, preDefineSetup, data.Guid);
        }

        public static UnaryOperatorNodeModel CreateUnaryStatementNode(this IGraphNodeCreationData data, UnaryOperatorKind kind)
        {
            return data.GraphModel.CreateUnaryOperatorNode(kind, data.Position, data.SpawnFlags, data.Guid);
        }

        public static UnaryOperatorNodeModel CreateUnaryStatementNode(this IStackedNodeCreationData data, UnaryOperatorKind kind)
        {
            return data.StackModel.CreateUnaryStatementNode(kind, data.Index, data.SpawnFlags, data.Guid);
        }

        public static BinaryOperatorNodeModel CreateBinaryOperatorNode(this IGraphNodeCreationData data, BinaryOperatorKind kind)
        {
            return data.GraphModel.CreateBinaryOperatorNode(kind, data.Position, data.SpawnFlags, data.Guid);
        }

        public static IVariableModel CreateVariableNode(this IGraphNodeCreationData data, IVariableDeclarationModel declarationModel)
        {
            return data.GraphModel.CreateVariableNode(declarationModel, data.Position, data.SpawnFlags, data.Guid);
        }

        public static IConstantNodeModel CreateConstantNode(this IGraphNodeCreationData data, string constantName, TypeHandle typeHandle)
        {
            return data.GraphModel.CreateConstantNode(constantName, typeHandle, data.Position, data.SpawnFlags, data.Guid);
        }

        public static ISystemConstantNodeModel CreateSystemConstantNode(this IGraphNodeCreationData data, Type declaringType, Type constantType, string constantName)
        {
            void Setup(SystemConstantNodeModel n)
            {
                n.ReturnType = constantType.GenerateTypeHandle(data.GraphModel.Stencil);
                n.DeclaringType = declaringType.GenerateTypeHandle(data.GraphModel.Stencil);
                n.Identifier = constantName;
            }

            var name = $"{declaringType.FriendlyName(false)} > {constantName}";
            return data.GraphModel.CreateNode<SystemConstantNodeModel>(name, data.Position, data.SpawnFlags, Setup, data.Guid);
        }

        public static SetPropertyGroupNodeModel CreateSetPropertyGroupNode(this IStackedNodeCreationData data)
        {
            return data.StackModel.CreateSetPropertyGroupNode(data.Index, data.SpawnFlags, data.Guid);
        }

        public static GetPropertyGroupNodeModel CreateGetPropertyGroupNode(this IGraphNodeCreationData data)
        {
            return data.GraphModel.CreateGetPropertyGroupNode(data.Position, data.SpawnFlags, data.Guid);
        }
    }
}