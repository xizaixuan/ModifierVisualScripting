using System;
using Modifier.NodeModels;
using Unity.Entities;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using ValueType = Modifier.Runtime.ValueType;

namespace Modifier.DotsStencil
{
    public static class SearcherFilterExtensions
    {
        public static SearcherFilter WithExecutionInputNodes(this SearcherFilter self)
        {
            self.RegisterNode(data => typeof(IHasMainExecutionInputPort).IsAssignableFrom(data.Type));
            return self;
        }

        public static SearcherFilter WithExecutionOutputNodes(this SearcherFilter self)
        {
            self.RegisterNode(data => typeof(IHasMainExecutionOutputPort).IsAssignableFrom(data.Type));
            return self;
        }

        //TODO simplistic, to improve.
        static bool IsRelevantMathNode(ValueType valueType, NodeSearcherItemData data)
        {
            if (data.Type != typeof(MathNodeModel))
                return false;
            switch (valueType)
            {
                case ValueType.Bool:
                case ValueType.Int:
                case ValueType.Float:
                case ValueType.Float2:
                case ValueType.Float3:
                case ValueType.Float4:
                case ValueType.Quaternion:
                    return true;
                default:
                    return false;
            }
        }

        public static SearcherFilter WithDataInputNodes(this SearcherFilter self, ValueType portType)
        {
            self.RegisterNode(data => IsRelevantMathNode(portType, data) || typeof(IHasMainInputPort).IsAssignableFrom(data.Type));
            return self;
        }

        public static SearcherFilter WithDataOutputNodes(this SearcherFilter self, ValueType portType)
        {
            self.RegisterNode(data => IsRelevantMathNode(portType, data) || typeof(IHasMainOutputPort).IsAssignableFrom(data.Type));
            return self;
        }

        public static SearcherFilter WithComponentTypes(this SearcherFilter self, Stencil stencil)
        {
            return self.WithTypesInheriting<IComponentData>(stencil);
        }

        public static SearcherFilter WithAuthoringComponentTypes(this SearcherFilter self, Stencil stencil)
        {
            return self.WithTypesInheriting<IComponentData, GenerateAuthoringComponentAttribute>(stencil);
        }

        public static SearcherFilter WithValueTypes(this SearcherFilter self)
        {
            self.RegisterType(x => x.Type.ToValueType(out var _));
            return self;
        }
    }
}
