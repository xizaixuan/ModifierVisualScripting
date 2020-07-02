using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Modifier.EditorCommon.Extensions;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Editor.SmartSearch
{
    public class SearcherFilter
    {
        public static SearcherFilter Empty { get; } = new SearcherFilter(SearcherContext.None);

        readonly SearcherContext m_Context;
        readonly Dictionary<string, List<Func<ISearcherItemData, bool>>> m_Filters;

        public SearcherFilter(SearcherContext context)
        {
            m_Context = context;
            m_Filters = new Dictionary<string, List<Func<ISearcherItemData, bool>>>();
        }

        static string GetFilterKey(SearcherContext context, SearcherItemTarget target)
        {
            return $"{context}_{target}";
        }

        public SearcherFilter WithEnums(Stencil stencil)
        {
            this.RegisterType(data => data.Type.GetMetadata(stencil).IsEnum);
            return this;
        }

        public SearcherFilter WithTypesInheriting<T>(Stencil stencil)
        {
            return WithTypesInheriting(stencil, typeof(T));
        }

        public SearcherFilter WithTypesInheriting<T, TA>(Stencil stencil) where TA : Attribute
        {
            return WithTypesInheriting(stencil, typeof(T), typeof(TA));
        }

        public SearcherFilter WithTypesInheriting(Stencil stencil, Type type, Type attributeType = null)
        {
            this.RegisterType(data =>
            {
                var dataType = data.Type.Resolve(stencil);
                return type.IsAssignableFrom(dataType) && (attributeType == null || dataType.GetCustomAttribute(attributeType) != null);
            });
            return this;
        }

        public SearcherFilter WithMacros()
        {
            this.RegisterGraphAsset(data => data.GraphAssetModel != null);
            return this;
        }

        public SearcherFilter WithGraphAsset(IGraphAssetModel assetModel)
        {
            this.RegisterGraphAsset(data => data.GraphAssetModel == assetModel);
            return this;
        }

        public SearcherFilter WithVariables(Stencil stencil, IPortModel portModel)
        {
            this.RegisterVariable(data =>
            {
                if (portModel.NodeModel is IOperationValidator operationValidator)
                    return operationValidator.HasValidOperationForInput(portModel, data.Type);

                return portModel.DataTypeHandle == TypeHandle.Unknown
                || portModel.DataTypeHandle.IsAssignableFrom(data.Type, stencil);
            });
            return this;
        }

        public SearcherFilter WithVisualScriptingNodes()
        {
            this.RegisterNode(data => data.Type != null);
            return this;
        }

        public SearcherFilter WithVisualScriptingNodes(Type type)
        {
            this.RegisterNode(data => type.IsAssignableFrom(data.Type));
            return this;
        }

        public SearcherFilter WithVisualScriptingNodes(IStackModel stackModel)
        {
            this.RegisterNode(data => data.Type != null && stackModel.AcceptNode(data.Type));
            return this;
        }

        public SearcherFilter WithVisualScriptingNodes(Type type, IStackModel stackModel)
        {
            this.RegisterNode(data => type.IsAssignableFrom(data.Type) && stackModel.AcceptNode(data.Type));
            return this;
        }

        public SearcherFilter WithVisualScriptingNodesExcept(IEnumerable<Type> exceptions)
        {
            this.RegisterNode(data => data.Type != null && !exceptions.Any(e => e.IsAssignableFrom(data.Type)));
            return this;
        }

        public SearcherFilter WithStickyNote()
        {
            this.RegisterStickyNote(data => true);
            return this;
        }

        public SearcherFilter WithStack()
        {
            this.RegisterStack(data => true);
            return this;
        }

        public SearcherFilter WithBinaryOperators()
        {
            this.RegisterBinaryOperator(data => true);
            return this;
        }

        public SearcherFilter WithBinaryOperators(Type type)
        {
            this.RegisterBinaryOperator(data => TypeSystem.GetOverloadedBinaryOperators(type).Contains(data.Kind));
            return this;
        }

        public SearcherFilter WithUnaryOperators()
        {
            this.RegisterUnaryOperator(data => true);
            return this;
        }

        public SearcherFilter WithUnaryOperators(Type type, bool isConstant = false)
        {
            this.RegisterUnaryOperator(data => !isConstant && TypeSystem.GetOverloadedUnaryOperators(type).Contains(data.Kind));
            return this;
        }

        public SearcherFilter WithConstants(Stencil stencil, IPortModel portModel)
        {
            this.RegisterConstant(data =>
            {
                if (portModel.NodeModel is IOperationValidator operationValidator)
                    return operationValidator.HasValidOperationForInput(portModel, data.Type);

                return portModel.DataTypeHandle == TypeHandle.Unknown
                || portModel.DataTypeHandle.IsAssignableFrom(data.Type, stencil);
            });
            return this;
        }

        public SearcherFilter WithConstants()
        {
            this.RegisterConstant(data => true);
            return this;
        }

        internal void Register<T>(Func<T, bool> filter, SearcherItemTarget target) where T : ISearcherItemData
        {
            bool Func(ISearcherItemData data) => filter.Invoke((T)data);
            var key = GetFilterKey(m_Context, target);

            if (!m_Filters.TryGetValue(key, out var searcherItemsData))
                m_Filters.Add(key, searcherItemsData = new List<Func<ISearcherItemData, bool>>());

            searcherItemsData.Add(Func);
        }

        internal bool ApplyFilters(ISearcherItemData data)
        {
            return m_Filters.TryGetValue(GetFilterKey(m_Context, data.Target), out var filters)
                && filters.Any(f => f.Invoke(data));
        }
    }
}