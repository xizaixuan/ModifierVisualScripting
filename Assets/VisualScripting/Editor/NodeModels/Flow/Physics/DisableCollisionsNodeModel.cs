using System;
using Modifier.Runtime;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace Modifier.DotsStencil
{
#if VS_DOTS_PHYSICS_EXISTS
    [Serializable, DotsSearcherItem("Physics/" + k_Title)]
    class DisableCollisionsNodeModel : DotsNodeModel<DisableCollisions>, IHasMainExecutionInputPort, IHasMainExecutionOutputPort
    {
        const string k_Title = "Disable Collisions";
        public override string Title => k_Title;
        public IPortModel ExecutionInputPort { get; set; }

        public IPortModel ExecutionOutputPort { get; set; }
    }
#endif
}
