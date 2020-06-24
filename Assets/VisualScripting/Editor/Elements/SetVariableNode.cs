using System;
using System.Linq;
using UnityEditor;
using Unity.Modifier.GraphElements;
using UnityEditor.Searcher;
using UnityEditor.Modifier.VisualScripting.Editor;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;
using UnityEngine.UIElements;
using Node = UnityEditor.Modifier.VisualScripting.Editor.Node;

namespace Modifier.DotsStencil
{
    class SetVariableNode : Node
    {
        class VariableSearcherItem : SearcherItem
        {
            public IVariableDeclarationModel declarationModel { get; }

            public VariableSearcherItem(IVariableDeclarationModel declarationModel)
                : base(declarationModel.Name)
            {
                this.declarationModel = declarationModel;
            }
        }

        SetVariableNodeModel SetVariableNodeModel => Model as SetVariableNodeModel;

        Pill m_Pill;

        protected override void BuildUI()
        {
            base.BuildUI();

            m_Pill = new Pill();

            var pillContainer = new VisualElement();
            pillContainer.AddToClassList(k_UssClassName + "__variable");
            pillContainer.Add(m_Pill);

            m_Pill.RegisterCallback<MouseDownEvent>(ShowSearcher);

            // make it clear the ux is not final
            var wipOverlay = new VisualElement { pickingMode = PickingMode.Ignore };
            wipOverlay.AddToClassList("vs-wip-node-overlay");
            Add(wipOverlay);

            TitleContainer?.Insert(1, pillContainer);
        }

        public override void UpdateFromModel()
        {
            base.UpdateFromModel();

            if (TitleLabel != null)
            {
                TitleLabel.Text = "Set";
            }

            var label = m_Pill.Q<Label>("title-label");
            label.text = SetVariableNodeModel.DeclarationModel?.Name ?? "<Pick a variable>";
        }

        void ShowSearcher(MouseDownEvent e)
        {
            if (NodeModel is SetVariableNodeModel model)
            {
                SearcherWindow.Show(EditorWindow.focusedWindow, ((VSGraphModel)model.GraphModel).GraphVariableModels.Where(g => GraphBuilder.GetVariableType(g) == GraphBuilder.VariableType.Variable)
                    .Select(v => (SearcherItem) new VariableSearcherItem(v)).ToList(), "Pick a variable to set", item =>
                    {
                        var variableSearcherItem = (item as VariableSearcherItem);
                        if (variableSearcherItem == null)
                            return true;
                        model.DeclarationModel = variableSearcherItem.declarationModel;
                        model.DefineNode();
                        Store.Dispatch(new RefreshUIAction(UpdateFlags.GraphTopology));
                        return true;
                    }, Event.current.mousePosition);
            }
        }
    }
}
