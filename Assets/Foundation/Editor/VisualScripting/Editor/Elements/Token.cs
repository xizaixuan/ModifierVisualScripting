using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor.ConstantEditor;
using UnityEditor.Modifier.VisualScripting.Editor.Highlighting;
using UnityEditor.Modifier.VisualScripting.Editor.Renamable;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class Token : TokenNode, IHighlightable, ICustomColor, IBadgeContainer, INodeState
    {
        public new INodeModel NodeModel => base.NodeModel as INodeModel;
        public new Store Store => base.Store as Store;

        SerializedObject m_SerializedObject;

        public IGraphElementModel GraphElementModel => NodeModel;

        VisualElement TokenEditor { get; set; }

        public override bool IsRenamable()
        {
            if (!base.IsRenamable())
                return false;

            if (NodeModel is Unity.GraphToolsFoundation.Model.IRenamable)
                return true;

            IVariableDeclarationModel declarationModel = (NodeModel as IVariableModel)?.DeclarationModel;
            return declarationModel is Unity.GraphToolsFoundation.Model.IRenamable;
        }

        internal static TypeHandle[] s_PropsToHideLabel =
        {
            TypeHandle.Int,
            TypeHandle.Float,
            TypeHandle.Vector2,
            TypeHandle.Vector3,
            TypeHandle.Vector4,
            TypeHandle.String
        };

        VisualElement m_ContentContainer;
        public override VisualElement contentContainer => m_ContentContainer ?? this;

        public void Setup(INodeModel model, IStore store, GraphView graphView, Texture2D icon = null)
        {
            Icon = icon;
            base.Setup(model as IGTFNodeModel, store, graphView);
        }

        protected override void BuildUI()
        {
            var selectionBorder = new VisualElement();
            selectionBorder.AddToClassList("ge-node__selection-border");
            Add(selectionBorder);

            var contentContainerElement = new VisualElement();
            contentContainerElement.AddToClassList("ge-node__content-container");
            selectionBorder.Add(contentContainerElement);
            m_ContentContainer = contentContainerElement;

            base.BuildUI();

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "PropertyField.uss"));
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "Node.uss"));
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UICreationHelper.templatePath + "Token.uss"));

            if (TitleLabel != null)
            {
                if (Model is IObjectReference modelReference)
                {
                    if (modelReference is IExposeTitleProperty titleProperty)
                    {
                        TitleLabel.BindingPath = titleProperty.TitlePropertyName;
                    }
                }
            }

            if (Model is ConstantNodeModel constantNodeModel)
            {
                SetupConstantEditor(constantNodeModel);
                AddToClassList(k_UssClassName + "--constant-token");
            }
            else if (Model is VariableNodeModel && TitleContainer != null)
            {
                var greenDot = new Image();
                greenDot.AddToClassList(Unity.GraphElements.Node.k_UssClassName + "__green-dot");
                TitleContainer.Insert(0, greenDot);
            }

            viewDataKey = NodeModel.GetId();

            this.AddOverlay();
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            if (Model is IVariableModel variableModel && variableModel.DeclarationModel != null)
            {
                switch (variableModel.DeclarationModel.Modifiers)
                {
                    case ModifierFlags.ReadOnly:
                        AddToClassList(k_UssClassName + "--read-only");
                        break;
                    case ModifierFlags.WriteOnly:
                        AddToClassList(k_UssClassName + "--write-only");
                        break;
                }
            }
            else if (Model is IEdgePortalEntryModel)
            {
                AddToClassList("portal-entry");
            }
            else if (Model is IEdgePortalExitModel)
            {
                AddToClassList("portal-exit");
            }

            if (Model is NodeModel nodeModel)
            {
                tooltip = $"{nodeModel.VariableString}";
                if (!string.IsNullOrEmpty(nodeModel.DataTypeString))
                    tooltip += $" of type {nodeModel.DataTypeString}";
                if (Model is IVariableModel currentVariableModel &&
                    !string.IsNullOrEmpty(currentVariableModel.DeclarationModel?.Tooltip))
                    tooltip += "\n" + currentVariableModel.DeclarationModel.Tooltip;

                if (nodeModel.HasUserColor)
                {
                    var border = this.MandatoryQ(className: "ge-node__content-container");
                    border.style.backgroundColor = nodeModel.Color;
                    border.style.backgroundImage = null;
                }
                else
                {
                    var border = this.MandatoryQ(className: "ge-node__content-container");
                    border.style.backgroundColor = StyleKeyword.Null;
                    border.style.backgroundImage = StyleKeyword.Null;
                }
            }

            UIState = NodeModel.State == ModelState.Disabled ? NodeUIState.Disabled : NodeUIState.Enabled;
            this.ApplyNodeState();
        }

        bool TokenEditorNeedsLabel
        {
            get
            {
                if (NodeModel is ConstantNodeModel constantNodeModel)
                    return !s_PropsToHideLabel.Contains(constantNodeModel.Type.GenerateTypeHandle(NodeModel.VSGraphModel.Stencil));
                return true;
            }
        }

        void SetupConstantEditor(ConstantNodeModel constantNodeModel)
        {
            void OnValueChanged(IChangeEvent evt)
            {
                if (constantNodeModel.OutputPort.IsConnected)
                    Store.Dispatch(new RefreshUIAction(UpdateFlags.RequestCompilation));
            }

            TokenEditor = this.CreateEditorForNodeModel((IConstantNodeModel)NodeModel, OnValueChanged);

            var icm = NodeModel as IStringWrapperConstantModel;

            if (!TokenEditorNeedsLabel && icm != null && TitleContainer != null)
                TitleContainer.style.display = DisplayStyle.None;

            Insert(1, TokenEditor);

            if (TitleLabel != null && icm != null)
            {
                TitleLabel.Text = icm.Label;
            }
        }

        bool m_IsMovable = true;
        public override bool IsMovable => m_IsMovable;

        public void SetMovable(bool movable)
        {
            m_IsMovable = movable;
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            ((VseGraphView)GraphView).ClearGraphElementsHighlight(ShouldHighlightItemUsage);
        }

        public bool Highlighted
        {
            get => ClassListContains("highlighted");
            set => EnableInClassList("highlighted", value);
        }

        public bool ShouldHighlightItemUsage(IGraphElementModel elementModel)
        {
            var currentVariableModel = NodeModel as IVariableModel;
            var currentEdgePortalModel = Model as IEdgePortalModel;
            // 'this' tokens have a null declaration model
            if (currentVariableModel?.DeclarationModel == null && currentEdgePortalModel == null)
                return NodeModel is ThisNodeModel && elementModel is ThisNodeModel;

            switch (elementModel)
            {
                case IVariableModel variableModel
                    when ReferenceEquals(variableModel.DeclarationModel, currentVariableModel?.DeclarationModel):
                case IVariableDeclarationModel variableDeclarationModel
                    when ReferenceEquals(variableDeclarationModel, currentVariableModel?.DeclarationModel):
                case IEdgePortalModel edgePortalModel
                    when ReferenceEquals(edgePortalModel.DeclarationModel, currentEdgePortalModel?.DeclarationModel):
                    return true;
            }

            return false;
        }

        public IconBadge ErrorBadge { get; set; }
        public ValueBadge ValueBadge { get; set; }
        public NodeUIState UIState { get; set; }
    }
}