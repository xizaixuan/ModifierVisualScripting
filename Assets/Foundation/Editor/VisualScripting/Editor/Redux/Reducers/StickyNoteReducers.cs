using System.Linq;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using Object = UnityEngine.Object;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    static class StickyNoteReducers
    {
        public static void Register(Store store)
        {
            store.Register<CreateStickyNoteAction>(CreateStickyNote);
            store.Register<ResizeStickyNoteAction>(ResizeStickyNote);
            store.Register<UpdateStickyNoteAction>(UpdateStickyNote);
            store.Register<UpdateStickyNoteThemeAction>(UpdateStickyNoteTheme);
            store.Register<UpdateStickyNoteTextSizeAction>(UpdateStickyNoteTextSize);
        }

        static State CreateStickyNote(State previousState, CreateStickyNoteAction action)
        {
            ((VSGraphModel)previousState.CurrentGraphModel).CreateStickyNote(action.Position);
            return previousState;
        }

        static State ResizeStickyNote(State previousState, ResizeStickyNoteAction action)
        {
            if (action.ResizeWhat == ResizeFlags.None)
                return previousState;

            Undo.RegisterCompleteObjectUndo((Object)previousState.AssetModel, "Resize Sticky Note");
            EditorUtility.SetDirty((Object)previousState.AssetModel);

            foreach (var noteModel in action.Models)
            {
                var newRect = noteModel.PositionAndSize;
                if ((action.ResizeWhat & ResizeFlags.Left) == ResizeFlags.Left)
                {
                    newRect.x = action.Value.x;
                }
                if ((action.ResizeWhat & ResizeFlags.Top) == ResizeFlags.Top)
                {
                    newRect.y = action.Value.y;
                }
                if ((action.ResizeWhat & ResizeFlags.Width) == ResizeFlags.Width)
                {
                    newRect.width = action.Value.width;
                }
                if ((action.ResizeWhat & ResizeFlags.Height) == ResizeFlags.Height)
                {
                    newRect.height = action.Value.height;
                }

                noteModel.PositionAndSize = newRect;
                previousState.MarkForUpdate(UpdateFlags.UpdateView, noteModel);
            }

            return previousState;
        }

        static State UpdateStickyNote(State previousState, UpdateStickyNoteAction action)
        {
            Undo.RegisterCompleteObjectUndo((Object)previousState.AssetModel, "Change Sticky Note Content");
            EditorUtility.SetDirty((Object)previousState.AssetModel);

            action.StickyNoteModel.Title = action.Title;
            action.StickyNoteModel.Contents = action.Contents;

            previousState.MarkForUpdate(UpdateFlags.UpdateView, action.StickyNoteModel);
            return previousState;
        }

        static State UpdateStickyNoteTheme(State previousState, UpdateStickyNoteThemeAction action)
        {
            Undo.RegisterCompleteObjectUndo((Object)previousState.AssetModel, "Change Sticky Note Theme");
            EditorUtility.SetDirty((Object)previousState.AssetModel);

            foreach (var noteModel in action.Models)
            {
                noteModel.Theme = action.Value;
                previousState.MarkForUpdate(UpdateFlags.UpdateView, noteModel);
            }

            return previousState;
        }

        static State UpdateStickyNoteTextSize(State previousState, UpdateStickyNoteTextSizeAction action)
        {
            Undo.RegisterCompleteObjectUndo((Object)previousState.AssetModel, "Change Sticky Note Font Size");
            EditorUtility.SetDirty((Object)previousState.AssetModel);

            foreach (var noteModel in action.Models)
            {
                noteModel.TextSize = action.Value;
                previousState.MarkForUpdate(UpdateFlags.UpdateView, noteModel);
            }

            return previousState;
        }
    }
}