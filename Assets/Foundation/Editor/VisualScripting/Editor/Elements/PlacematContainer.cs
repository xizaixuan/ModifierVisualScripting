using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class PlacematContainer : Unity.Modifier.GraphElements.PlacematContainer
    {
        readonly Store m_Store;

        public PlacematContainer(Store store, GraphView gv)
            : base(gv)
        {
            m_Store = store;
        }

        protected override void UpdateElementsOrder()
        {
            var origOrders = Placemats.ToDictionary(pm => pm, pm => pm.ZOrder);
            base.UpdateElementsOrder();

            var changed = Placemats.Where(pm => origOrders[pm] != pm.ZOrder).OfType<Placemat>().ToList();
            m_Store.Dispatch(
                new ChangePlacematZOrdersAction(
                    changed.Select(pm => pm.ZOrder).ToArray(),
                    changed.Select(pm => pm.GraphElementModel).Cast<PlacematModel>().ToArray()));
        }
    }
}