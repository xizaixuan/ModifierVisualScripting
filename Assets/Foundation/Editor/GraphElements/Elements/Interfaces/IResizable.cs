namespace Unity.Modifier.GraphElements
{
    [Flags]
    public enum ResizeFlags
    {
        None = 0,
        Left = 1,
        Top = 2,
        Width = 4,
        Height = 8,
        All = Left | Top | Width | Height,
    };

    public interface IResizable
    {
        void OnResized(Rect newRect, ResizeFlags resizeWhat);
    }
}