using Unity.Modifier.GraphToolsFoundation.Model;

namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IGraphAssetModel : IGTFGraphAssetModel
    {
        string Name { get; }

        bool IsSameAsset(IGraphAssetModel otherGraphAssetModel);
    }
}