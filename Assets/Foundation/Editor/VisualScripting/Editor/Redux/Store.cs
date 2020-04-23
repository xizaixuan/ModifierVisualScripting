using UnityEditor.Modifier.EditorCommon.Redux;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class Store : Store<State>
    {
        public enum Options
        {
            None,
        }

        readonly Options m_Options;

        public Store(State initialState = null, Options options = Options.None)
            : base(initialState)
        {
            m_Options = options;

            RegisterReducers();
        }

        public void RegisterReducers()
        {
            // Register reducers.
            GraphAssetReducers.Register(this);
        }
    }
}