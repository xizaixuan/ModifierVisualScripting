using System;
using System.Collections.Generic;
using System.Linq;
using Modifier.DotsStencil;
using Unity.Mathematics;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine.UIElements;
using Node = UnityEditor.Modifier.VisualScripting.Editor.Node;

namespace Modifier.Elements
{
    public class DotsNode : Node, IContextualMenuBuilder
    {
        void IContextualMenuBuilder.BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            BuildContextualMenu(evt);
        }

        protected override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            // TODO find a proper solution - contextual inspectors ?
            var variableCountFields = NodeModel.GetType().GetFields()
                .Select(f => (Field: f, Attribute: (HackContextualMenuVariableCountAttribute)Attribute.GetCustomAttribute(f, typeof(HackContextualMenuVariableCountAttribute))))
                .Where(f => f.Attribute != null);
            foreach (var variableCountField in variableCountFields)
            {
                var fieldInfo = variableCountField.Field;
                if (fieldInfo.FieldType != typeof(int))
                    throw new InvalidOperationException("VariableCountAttribute is only supported on int fields");
                var portDesc = variableCountField.Attribute.Description;
                if (NodeModel is BaseDotsNodeModel dotsNodeModel
                    && dotsNodeModel.PortCountData.TryGetValue(fieldInfo.Name, out var customPortDesc))
                    portDesc = customPortDesc;
                var itemName = portDesc.Name ?? fieldInfo.Name;
                int max = portDesc.Max;
                evt.menu.AppendAction($"Add {itemName}", action: action =>
                {
                    fieldInfo.SetValue(NodeModel, (int)fieldInfo.GetValue(NodeModel) + 1);
                    ((NodeModel)NodeModel).DefineNode();
                    Store.Dispatch(new RefreshUIAction(UpdateFlags.GraphTopology, new List<IGraphElementModel> { NodeModel }));
                }, action =>
                    {
                        int value = (int)fieldInfo.GetValue(NodeModel);
                        return max == -1 || value < max ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    });
                int min = math.max(0, portDesc.Min);
                evt.menu.AppendAction($"Remove {itemName}", action: action =>
                {
                    fieldInfo.SetValue(NodeModel, Math.Max(0, (int)fieldInfo.GetValue(NodeModel) - 1));
                    ((NodeModel)NodeModel).DefineNode();
                    Store.Dispatch(new RefreshUIAction(UpdateFlags.GraphTopology, new List<IGraphElementModel> { NodeModel }));
                }, action =>
                    {
                        int value = (int)fieldInfo.GetValue(NodeModel);
                        return value > min ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                    });
            }
        }
    }
}
