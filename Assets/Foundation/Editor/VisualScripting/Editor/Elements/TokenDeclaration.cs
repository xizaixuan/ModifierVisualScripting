﻿using System;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor.Highlighting;
using UnityEditor.Modifier.VisualScripting.Editor.Renamable;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class TokenDeclaration : GraphElement, IDroppable, IHighlightable, IRenamable, IMovable
    {
        readonly Pill m_Pill;
        TextField m_TitleTextfield;

        public new Store Store => base.Store as Store;

        public string TitleValue => Declaration.Title.Nicify();

        public VisualElement TitleEditor => m_TitleTextfield ?? (m_TitleTextfield = new TextField { name = "titleEditor", isDelayed = true });

        public bool EditTitleCancelled { get; set; } = false;

        public VisualElement TitleElement => this;
        public IVariableDeclarationModel Declaration => Model as IVariableDeclarationModel;

        public bool IsFramable() => true;

        public RenameDelegate RenameDelegate => null;

        public void SetClasses()
        {
            ClearClassList();
            AddToClassList("token");
            AddToClassList("declaration");

            AddToClassList(Declaration?.VariableType == VariableType.GraphVariable ? "graphVariable" : "functionVariable");
        }

        public TokenDeclaration(Store store, IVariableDeclarationModel model, GraphView graphView)
        {
            m_Pill = new Pill();
            Add(m_Pill);

            if (model is IObjectReference modelReference)
            {
                if (modelReference is IExposeTitleProperty titleProperty)
                {
                    var titleLabel = m_Pill.Q<Label>("title-label");
                    titleLabel.bindingPath = titleProperty.TitlePropertyName;
                }
            }

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "Token.uss"));

            Setup(model as IGTFGraphElementModel, store, graphView);

            m_Pill.icon = Declaration.IsExposed
                ? GraphViewStaticBridge.LoadIconRequired("GraphView/Nodes/BlackboardFieldExposed.png")
                : null;

            m_Pill.text = Declaration.Title;

            var variableModel = model as VariableDeclarationModel;
            Stencil stencil = store.GetState().CurrentGraphModel?.Stencil;
            if (variableModel != null && stencil != null && variableModel.DataType.IsValid)
            {
                string friendlyTypeName = variableModel.DataType.GetMetadata(stencil).FriendlyName;
                Assert.IsTrue(!string.IsNullOrEmpty(friendlyTypeName));
                tooltip = $"{variableModel.VariableString} declaration of type {friendlyTypeName}";
                if (!string.IsNullOrEmpty(variableModel.Tooltip))
                    tooltip += "\n" + variableModel.Tooltip;
            }

            SetClasses();

            this.EnableRename();

            if (model != null)
                viewDataKey = model.GetId();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            (GraphView as VseGraphView).ClearGraphElementsHighlight(ShouldHighlightItemUsage);
        }

        public bool ShouldHighlightItemUsage(IGraphElementModel model)
        {
            switch (model)
            {
                case IVariableModel variableModel
                    when ReferenceEquals(variableModel.DeclarationModel, Declaration):
                case IVariableDeclarationModel variableDeclarationModel
                    when ReferenceEquals(variableDeclarationModel, Declaration):
                    return true;
            }

            return false;
        }

        public override bool IsSelected(VisualElement selectionContainer)
        {
            var gView = selectionContainer?.GetFirstOfType<GraphView>();
            return gView != null && gView.selection.Contains(this);
        }

        public void UpdatePinning()
        {
        }

        public bool IsMovable => false;

        public int FindIndexInParent()
        {
            // Find index of child so we can provide with the correct information
            for (int i = 0; i < parent?.childCount; i++)
            {
                if (this == parent.ElementAt(i))
                    return i;
            }
            return -1;
        }

        public TokenDeclaration Clone()
        {
            var clone = new TokenDeclaration(Store, Declaration, GraphView)
            {
                viewDataKey = Guid.NewGuid().ToString()
            };
            return clone;
        }

        public IGraphElementModel GraphElementModel => Declaration;

        public bool Highlighted
        {
            get => m_Pill.highlighted;
            set => m_Pill.highlighted = value;
        }
    }
}