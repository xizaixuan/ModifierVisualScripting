using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model.Compilation
{
    public interface IBuilder
    {
        void Build(IEnumerable<GraphAssetModel> vsGraphAssetModels,
            Action<string, CompilerMessage[]> roslynCompilationOnBuildFinished);
    }
}