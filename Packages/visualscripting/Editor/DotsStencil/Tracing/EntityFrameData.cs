using System;
using System.Collections.Generic;
using UnityEditor.Modifier.VisualScripting.Editor.Plugins;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;

namespace Modifier.DotsStencil
{
    /// <summary>
    /// See <see cref="IFrameData"/>
    /// </summary>
    public class EntityFrameData : IFrameData, IDisposable
    {
        internal class FrameDataComparer : IComparer<EntityFrameData>
        {
            public int Compare(EntityFrameData x, EntityFrameData y)
            {
                if (x == null || y == null)
                    return x == y ? 0 : x == null ? -1 : 1;
                return Comparer<int>.Default.Compare(x.Frame, y.Frame);
            }
        }
        public int Frame { get; }

        public IEnumerable<TracingStep> GetDebuggingSteps(Stencil stencil)
        {
            if (!FrameTrace.IsValid)
                yield break;
            var reader = FrameTrace.AsReader();
            reader.BeginForEachIndex(0);
            var debugger = (DotsDebugger)stencil.Debugger;
            while (reader.RemainingItemCount != 0)
                if (debugger.ReadDebuggingDataModel(ref reader, this, out TracingStep step))
                    yield return step;

            reader.EndForEachIndex();
        }

        public DotsFrameTrace FrameTrace { get; }

        public EntityFrameData(int frame, DotsFrameTrace frameTrace)
        {
            Frame = frame;
            FrameTrace = frameTrace;
        }

        public void Dispose()
        {
            FrameTrace?.Dispose();
        }
    }
}
