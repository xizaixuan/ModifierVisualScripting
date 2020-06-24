using UnityEditor.Modifier.VisualScripting.Editor;
using Modifier.VisualScripting.Editor.Elements.Interfaces;

namespace Modifier.DotsStencil
{
    public class DotsToolbarProvider : IToolbarProvider
    {
        public bool ShowButton(string buttonName)
        {
            if (buttonName == VseMenu.BuildAllButton)
            {
                return false;
            }
            return buttonName != VseMenu.ViewInCodeViewerButton;
        }
    }
}
