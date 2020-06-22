using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IEdgePortalModel : INodeModel
    {
        int EvaluationOrder { get; }
        IVariableDeclarationModel DeclarationModel { get; }

        bool CanCreateOppositePortal();
    }

    public interface IEdgePortalEntryModel : IEdgePortalModel, IHasSingleInputPort
    {
        IPortModel InputPort { get; }
    }

    public interface IEdgePortalExitModel : IEdgePortalModel, IHasSingleOutputPort
    {
        IPortModel OutputPort { get; }
    }

    public interface IExecutionEdgePortalModel : IEdgePortalModel
    {
    }

    public interface IDataEdgePortalModel : IEdgePortalModel
    {
    }
}