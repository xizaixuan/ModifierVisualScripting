using System;
using System.Collections.Generic;

namespace Unity.Modifier.GraphElements
{
    public interface ICollectibleElement
    {
        void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFuc);
    }
}