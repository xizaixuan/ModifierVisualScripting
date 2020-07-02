using Modifier.VisualScripting.Editor.Elements.Interfaces;

namespace Modifier.VisualScripting.Editor.Elements
{
    public class ToolbarProvider : IToolbarProvider
    {
        public bool ShowButton(string buttonName)
        {
            return true;
        }
    }
}