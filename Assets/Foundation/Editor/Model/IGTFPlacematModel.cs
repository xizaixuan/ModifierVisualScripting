﻿using System.Collections.Generic;
using UnityEngine;

namespace Unity.Modifier.GraphToolsFoundation.Model
{
    public interface IGTFPlacematModel : IGTFGraphElementModel, IHasTitle, ISelectable, IPositioned, IDeletable, ICopiable, ICollapsible, IResizable, IRenamable
    {
        Color Color { get; set; }
        int ZOrder { get; set; }
        IEnumerable<IGTFGraphElementModel> HiddenElements { get; set; }
    }
}
