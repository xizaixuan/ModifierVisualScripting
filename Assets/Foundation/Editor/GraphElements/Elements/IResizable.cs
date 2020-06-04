namespace Unity.Modifier.GraphElements
{
    public interface IResizable
    {
        void OnStartResize();

        void OnResized();
    }
}