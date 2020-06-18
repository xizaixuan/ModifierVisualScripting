using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface IFunctionModel : IStackModel, IHasVariableDeclaration
    {
        IEnumerable<IVariableDeclarationModel> FunctionVariableModels { get; }
        IEnumerable<IVariableDeclarationModel> FunctionParameterModels { get; }
        TypeHandle ReturnType { get; }
        bool IsEntryPoint { get; }
        string CodeTitle { get; }
        bool AllowChangesToModel { get; }
        bool AllowMultipleInstances { get; }
        bool EnableProfiling { get; }
        bool HasReturnType { get; }
    }
}