namespace Modifier.Runtime.Mathematics
{
    public delegate Value MathValueDelegate(Value[] @params);

    public static class MathematicsExtensions
    {
        static MathValueDelegate GetDelegate(MathGeneratedFunction function)
        {
            return MathGeneratedDelegates.s_Delegates[function];
        }

        public static Value ApplyBinMath(this IGraphInstance graphInstance, InputDataMultiPort inputPort, MathGeneratedFunction binFunction)
        {
            var dataCount = inputPort.DataCount;
            Value[] values = new Value[dataCount];
            for (uint i = 0; i < dataCount; i++)
            {
                values[i] = graphInstance.ReadValue(inputPort.SelectPort(i));
            }
            var mathDelegate = GetDelegate(binFunction);
            return mathDelegate(values);
        }
    }
}
