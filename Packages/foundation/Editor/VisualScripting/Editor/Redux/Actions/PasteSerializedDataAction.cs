using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public struct TargetInsertionInfo
    {
        public Vector2 Delta;
        public StackBaseModel TargetStack;
        public int TargetStackInsertionIndex;
        public string OperationName;
    }

    public class PasteSerializedDataAction : IAction
    {
        public readonly VSGraphModel Graph;
        public readonly TargetInsertionInfo Info;
        public readonly IEditorDataModel EditorDataModel;
        public readonly VseGraphView.CopyPasteData Data;

        public PasteSerializedDataAction(VSGraphModel graph, TargetInsertionInfo info, IEditorDataModel editorDataModel, VseGraphView.CopyPasteData data)
        {
            Graph = graph;
            Info = info;
            EditorDataModel = editorDataModel;
            Data = data;
        }
    }
}