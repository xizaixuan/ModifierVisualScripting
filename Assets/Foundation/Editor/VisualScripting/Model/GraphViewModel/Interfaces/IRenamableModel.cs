namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IRenamableModel : IGraphElementModel
    {
        void Rename(string newName);
    }
}