using JetBrains.Annotations;
using System;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model.Stencils
{
    [PublicAPI]
    public class ClassSearcherFilterProvider : ISearcherFilterProvider
    {
        readonly Stencil m_Stencil;

        public ClassSearcherFilterProvider(Stencil stencil)
        {
            m_Stencil = stencil;
        }

        public virtual SearcherFilter GetGraphSearcherFilter()
        {
            return new SearcherFilter(SearcherContext.Graph)
                .WithVisualScriptingNodes()
                .WithStack()
                .WithBinaryOperators()
                .WithUnaryOperators()
                .WithConstants()
                .WithMacros()
                .WithStickyNote();
        }

        public virtual SearcherFilter GetStackSearcherFilter(IStackModel stackModel)
        {
            return new SearcherFilter(SearcherContext.Stack)
                .WithVisualScriptingNodes(stackModel)
                .WithUnaryOperators()
                .WithMacros();
        }

        public virtual SearcherFilter GetOutputToGraphSearcherFilter(IPortModel portModel)
        {
            // TODO : Need to be handled by TypeHandle.Resolve
            TypeHandle typeHandle = portModel.DataTypeHandle == TypeHandle.ThisType ? m_Stencil.GetThisType() : portModel.DataTypeHandle;
            Type type = typeHandle.Resolve(m_Stencil);
            GraphAssetModel assetModel = portModel.AssetModel as GraphAssetModel;

            return new SearcherFilter(SearcherContext.Graph)
                .WithVisualScriptingNodes()
                .WithUnaryOperators(type, portModel.NodeModel is IConstantNodeModel)
                .WithBinaryOperators(type)
                .WithGraphAsset(assetModel);
        }

        public virtual SearcherFilter GetOutputToStackSearcherFilter(IPortModel portModel, IStackModel stackModel)
        {
            // TODO : Need to be handled by TypeHandle.Resolve
            TypeHandle typeHandle = portModel.DataTypeHandle == TypeHandle.ThisType ? m_Stencil.GetThisType() : portModel.DataTypeHandle;
            Type type = typeHandle.Resolve(m_Stencil);
            GraphAssetModel assetModel = portModel.AssetModel as GraphAssetModel;

            return new SearcherFilter(SearcherContext.Stack)
                .WithVisualScriptingNodes()
                .WithUnaryOperators(type)
                .WithGraphAsset(assetModel);
        }

        public virtual SearcherFilter GetInputToGraphSearcherFilter(IPortModel portModel)
        {
            var dataType = portModel.DataTypeHandle.Resolve(m_Stencil);
            return new SearcherFilter(SearcherContext.Graph)
                .WithVisualScriptingNodesExcept(new[] { typeof(GetPropertyGroupNodeModel) })
                .WithVariables(m_Stencil, portModel)
                .WithConstants(m_Stencil, portModel)
                .WithUnaryOperators(dataType)
                .WithBinaryOperators(dataType);
        }

        public virtual SearcherFilter GetEdgeSearcherFilter(IEdgeModel edgeModel)
        {
            Type it = edgeModel.InputPortModel.DataTypeHandle.Resolve(m_Stencil);
            IPortModel opm = edgeModel.OutputPortModel;
            TypeHandle oth = opm.DataTypeHandle == TypeHandle.ThisType ? m_Stencil.GetThisType() : opm.DataTypeHandle;
            Type ot = oth.Resolve(m_Stencil);

            return new SearcherFilter(SearcherContext.Graph)
                .WithVisualScriptingNodesExcept(new[] { typeof(ThisNodeModel) }) // TODO : We should be able to determine if a VSNode type has input port instead of doing this
                .WithUnaryOperators(ot, opm.NodeModel is IConstantNodeModel)
                .WithBinaryOperators(ot);
        }

        public virtual SearcherFilter GetTypeSearcherFilter()
        {
            return SearcherFilter.Empty;
        }
    }
}