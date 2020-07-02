using System;
using System.Linq;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using PortType = UnityEditor.Modifier.VisualScripting.Model.PortType;

namespace Modifier.DotsStencil
{
    public class DotsSearcherFilterProvider : ISearcherFilterProvider
    {
        readonly Stencil m_Stencil;

        public DotsSearcherFilterProvider(Stencil stencil)
        {
            m_Stencil = stencil;
        }

        public virtual SearcherFilter GetGraphSearcherFilter()
        {
            var filter = new SearcherFilter(SearcherContext.Graph)
                .WithStickyNote();
            return m_Stencil is DotsStencil dotsStencil && dotsStencil.Type == DotsStencil.GraphType.Subgraph
                ? filter.WithVisualScriptingNodesExcept(Enumerable.Repeat<Type>(typeof(IEntryPointNode), 1))
                : filter.WithVisualScriptingNodes();
        }

        public SearcherFilter GetStackSearcherFilter(IStackModel stackModel)
        {
            throw new NotImplementedException();
        }

        public SearcherFilter GetOutputToStackSearcherFilter(IPortModel portModel, IStackModel stackModel)
        {
            throw new NotImplementedException();
        }

        public SearcherFilter GetOutputToGraphSearcherFilter(IPortModel portModel)
        {
            var filter = new SearcherFilter(SearcherContext.Graph);
            if (portModel.PortType == PortType.Execution)
                filter = filter.WithExecutionInputNodes();
            else if (portModel.PortType == PortType.Data)
                filter = filter.WithDataInputNodes(portModel.DataTypeHandle.ToValueTypeOrUnknown());

            // TODO decide what to do with output data nodes.
            return filter;
        }

        public SearcherFilter GetInputToGraphSearcherFilter(IPortModel portModel)
        {
            var filter = new SearcherFilter(SearcherContext.Graph);
            if (portModel.PortType == PortType.Execution)
                filter = filter.WithExecutionOutputNodes();
            else if (portModel.PortType == PortType.Data)
                filter = filter.WithDataOutputNodes(portModel.DataTypeHandle.ToValueTypeOrUnknown());
            return filter;
        }

        public SearcherFilter GetTypeSearcherFilter()
        {
            return SearcherFilter.Empty;
        }

        public SearcherFilter GetEdgeSearcherFilter(IEdgeModel edgeModel)
        {
            throw new NotImplementedException();
        }
    }
}
