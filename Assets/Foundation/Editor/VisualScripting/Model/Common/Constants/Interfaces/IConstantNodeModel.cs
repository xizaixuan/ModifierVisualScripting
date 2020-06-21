using System;
using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Model
{
    public interface IConstantNodeModel : IHasMainOutputPort, IHasSingleOutputPort
    {
        object ObjectValue { get; }
        bool IsLocked { get; }
        Type Type { get; }
    }

    public interface IStringWrapperConstantModel : IConstantNodeModel
    {
        List<string> GetAllInputNames();
        string StringValue { get; }
        string Label { get; }
    }
}