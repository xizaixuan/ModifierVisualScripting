using Modifier.DotsStencil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Modifier.NodeModels;
using Modifier.Runtime;
using Modifier.Runtime.Mathematics;
using Unity.Entities;
using Unity.Properties;
using Unity.Properties.UI;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEditor.Modifier.VisualScripting.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using PropertyElement = Unity.Properties.UI.PropertyElement;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [UsedImplicitly]
    class InputDataMultiPortInspector : Inspector<InputDataMultiPort>
    {
        public override VisualElement Build() => null;
    }

    [UsedImplicitly]
    class OutputDataMultiPortInspector : Inspector<OutputDataMultiPort>
    {
        public override VisualElement Build() => null;
    }

    [UsedImplicitly]
    class InputTriggerMultiPortInspector : Inspector<InputTriggerMultiPort>
    {
        public override VisualElement Build() => null;
    }

    [UsedImplicitly]
    class OutputTriggerMultiPortInspector : Inspector<OutputTriggerMultiPort>
    {
        public override VisualElement Build() => null;
    }
    [UsedImplicitly]
    class InputDataPortInspector : Inspector<InputDataPort>
    {
        public override VisualElement Build() => null;
    }

    [UsedImplicitly]
    class OutputDataPortInspector : Inspector<OutputDataPort>
    {
        public override VisualElement Build() => null;
    }

    [UsedImplicitly]
    class InputTriggerPortInspector : Inspector<InputTriggerPort>
    {
        public override VisualElement Build() => null;
    }

    [UsedImplicitly]
    class OutputTriggerPortInspector : Inspector<OutputTriggerPort>
    {
        public override VisualElement Build() => null;
    }

    abstract class PickFromSearcherInspector<T> : Inspector<T>
    {
        private Button m_Button;

        protected abstract string GetButtonText();

        protected abstract void ShowSearcher(Vector2 mousePosition, INodeModel nodeModel, Action<T> notifyChanged);

        // hack because internal
        private PropertyElement Root => typeof(Inspector<T>)
        .GetProperty("Root", BindingFlags.Instance | BindingFlags.NonPublic)
        .GetValue(this) as PropertyElement;

        public override VisualElement Build()
        {
            var visualElement = new VisualElement();
            visualElement.AddToClassList("unity-base-field");

            m_Button = new Button(OnClick) {text = GetButtonText()};
            m_Button.AddToClassList("unity-base-field__input");

            var label = new Label(DisplayName) {tooltip = Tooltip};
            label.AddToClassList("unity-base-field__label");

            visualElement.Add(label);
            visualElement.Add(m_Button);
            return visualElement;
        }

        private void OnClick()
        {
            var mousePosition = Event.current.mousePosition;

            // TODO ugly, see matching hack in VSEWindow
            INodeModel nodeModel = (INodeModel)Root.userData;

            ShowSearcher(mousePosition, nodeModel, x =>
            {
                Target = x;
                m_Button.text = GetButtonText();
                NotifyChanged();
            });
        }
    }

    [UsedImplicitly]
    class MathGeneratedFunctionInspector : PickFromSearcherInspector<MathGenericNode.MathGeneratedNodeSerializable>
    {
        protected override string GetButtonText()
        {
            return OpTitle(Target.Function.GetMethodsSignature());
        }

        protected override void ShowSearcher(Vector2 mousePosition, INodeModel nodeModel,
            Action<MathGenericNode.MathGeneratedNodeSerializable> notifyChanged)
        {
            var currentMethodName = Target.Function.GetMethodsSignature().OpType;
            var opSignatures = MathOperationsMetaData.MethodsByName[currentMethodName];

            void OnValuePicked(string s, int i)
            {
                if (MathOperationsMetaData.EnumForSignature.TryGetValue(opSignatures[i], out var value))
                    notifyChanged(new MathGenericNode.MathGeneratedNodeSerializable { Function = value});
            }

            SearcherService.ShowValues("Types", opSignatures.Select(OpTitle), mousePosition, OnValuePicked);
        }

        static string OpTitle(MathOperationsMetaData.OpSignature op)
        {
            return $"{op.OpType}({string.Join(", ", op.Params.Select(p => p.ToString()))})";
        }
    }

    [UsedImplicitly]
    class TypeReferenceInspector : PickFromSearcherInspector<TypeReference>
    {
        protected override string GetButtonText()
        {
            return Target.GetComponentType().GetManagedType()?.Name ?? "None";
        }

        protected override void ShowSearcher(Vector2 mousePosition, INodeModel nodeModel,
            Action<TypeReference> notifyChanged)
        {
            SearcherService.ShowTypes(nodeModel.VSGraphModel.Stencil, mousePosition, (handle, i) =>
            {
                var typeReference = Target;
                typeReference.TypeHash = TypeHash.CalculateStableTypeHash(handle.Resolve(nodeModel.VSGraphModel.Stencil));
                notifyChanged(typeReference);
            }, MakeSearcherAdapterFromAttribute(null, nodeModel));
        }

        SearcherFilter MakeSearcherAdapterFromAttribute(TypeSearcherAttribute attribute, INodeModel model)
        {
            var stencil = model.VSGraphModel.Stencil;
            var typeSearcher = new SearcherFilter(SearcherContext.Type);
            if (attribute is ComponentSearcherAttribute componentSearcher)
                return componentSearcher.ComponentOptions == ComponentOptions.OnlyAuthoringComponents
                    ? typeSearcher.WithAuthoringComponentTypes(stencil)
                    : typeSearcher.WithComponentTypes(stencil);
            return typeSearcher.WithTypesInheriting(stencil, attribute?.FilteredType ?? typeof(IComponentData));
        }
    }
}
