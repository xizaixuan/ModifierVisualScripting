using System.Collections.Generic;
using Unity.Modifier.GraphElements;

namespace Unity.Modifier.GraphToolsFoundation.Model
{
    public interface IGTFEditorDataModel
    {
        UpdateFlags UpdateFlags { get; }
        void SetUpdateFlag(UpdateFlags flag);
        IEnumerable<IGTFGraphElementModel> ModelsToUpdate { get; }
        void AddModelToUpdate(IGTFGraphElementModel controller);
        void ClearModelsToUpdate();
    }
}
