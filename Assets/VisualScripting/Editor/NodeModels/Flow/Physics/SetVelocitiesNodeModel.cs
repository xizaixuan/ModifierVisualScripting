using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
#if VS_DOTS_PHYSICS_EXISTS
    [Serializable, DotsSearcherItem("Physics/" + k_Title)]
    class SetVelocitiesNodeModel : DotsNodeModel<SetVelocities>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort, IHasMainInputPort
    {
        const string k_Title = "Set Velocities";
        public override string Title => k_Title;
        public IPortModel ExecutionInputPort { get; set; }

        public IPortModel ExecutionOutputPort { get; set; }
        public IPortModel InputPort { get; set; }
    }
#endif
}
