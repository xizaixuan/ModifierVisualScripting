using System;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model.Stencils
{
    public class MacroSearcherFilterProvider : ISearcherFilterProvider
    {
        readonly Stencil m_Stencil;

        public MacroSearcherFilterProvider(Stencil stencil)
        {
            m_Stencil = stencil;
        }

        public SearcherFilter GetGraphSearcherFilter()
        {
            return new SearcherFilter(SearcherContext.Graph)
                .WithVisualScriptingNodes()
                .WithBinaryOperators()
                .WithUnaryOperators()
                .WithConstants()
                .WithMacros()
                .WithStickyNote();
        }

        public SearcherFilter GetStackSearcherFilter(IStackModel stackModel)
        {
            throw new NotImplementedException("Macro does not support stacks");
        }

        public SearcherFilter GetOutputToGraphSearcherFilter(IPortModel portModel)
        {
            // TODO : Need to be handled by TypeHandle.Resolve
            TypeHandle typeHandle = portModel.DataTypeHandle == TypeHandle.ThisType ? m_Stencil.GetThisType() : portModel.DataTypeHandle;
            Type type = typeHandle.Resolve(m_Stencil);
            VSGraphAssetModel assetModel = portModel.AssetModel as VSGraphAssetModel;

            return new SearcherFilter(SearcherContext.Graph)
                .WithVisualScriptingNodes()
                .WithUnaryOperators(type, portModel.NodeModel is IConstantNodeModel)
                .WithBinaryOperators(type)
                .WithGraphAsset(assetModel);
        }

        public SearcherFilter GetOutputToStackSearcherFilter(IPortModel portModel, IStackModel stackModel)
        {
            throw new NotImplementedException("Macro does not support stacks");
        }

        public SearcherFilter GetInputToGraphSearcherFilter(IPortModel portModel)
        {
            return new SearcherFilter(SearcherContext.Graph)
                .WithVisualScriptingNodes()
                .WithVariables(m_Stencil, portModel)
                .WithConstants(m_Stencil, portModel);
        }

        public SearcherFilter GetTypeSearcherFilter()
        {
            return SearcherFilter.Empty;
        }

        public SearcherFilter GetEdgeSearcherFilter(IEdgeModel edgeModel)
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
    }
}