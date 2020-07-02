using System;
using System.Reflection;
using UnityEditor.Modifier.EditorCommon.Redux;

namespace Unity.Modifier.GraphElements
{
    public class ElementBuilder
    {
        public GraphView GraphView { get; set; }

        public static Type KeySelector(MethodInfo x)
        {
            return x.GetParameters()[2].ParameterType;
        }

        public static bool FilterMethods(MethodInfo x)
        {
            if (x.ReturnType != typeof(IGraphElement))
                return false;

            var parameters = x.GetParameters();
            return parameters.Length == 3 && parameters[1].ParameterType == typeof(IStore);
        }
    }
}