using UnityEditor;

namespace Modifier.Runtime
{
    public static class StartStopPauseResetBehaviour
    {
        public static Execution Update(ref bool running, ref float elapsed, float deltaTime, float duration, out bool updated, out bool finished)
        {
            updated = false;
            if (running)
            {
                elapsed += deltaTime;
                updated = true;
            }

            return CheckCompletion(ref running, ref elapsed, duration, out finished);
        }

        public static Execution CheckCompletion(
            ref bool running,
            ref float elapsed,
            float duration,
            out bool finished)
        {
            finished = false;
            if (elapsed >= duration)
            {
                if (running)
                {
                    running = false;
                    elapsed = 0;
                    finished = true;
                }

                return Execution.Done;
            }

            return running ? Execution.Running : Execution.Done;
        }

        public static void Execute(in InputTriggerPort triggeredPort,
            in InputTriggerPort start,
            in InputTriggerPort stop,
            in InputTriggerPort pause,
            in InputTriggerPort reset,
            ref bool running,
            ref float elapsed)
        {
            if (triggeredPort == start)
            {
                running = true;
            }
            else if (triggeredPort == pause)
            {
                running = false;
            }
            else if (triggeredPort == reset)
            {
                elapsed = 0;
            }
            else if (triggeredPort == stop)
            {
                running = false;
                elapsed = 0;
            }
        }
    }
}
