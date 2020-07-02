using System;

namespace Modifier.DotsStencil
{
    /// <summary>
    /// Attribute to add "Add x/Remove x" menu items to a node context menu (eg. "Add/Remove switch case"
    /// </summary>
    class HackContextualMenuVariableCountAttribute : Attribute
    {
        public BaseDotsNodeModel.PortCountProperties Description;

        public HackContextualMenuVariableCountAttribute(string name, int min = 0, int max = -1)
        {
            Description = new BaseDotsNodeModel.PortCountProperties { Max = max, Min = min, Name = name };
        }
    }
}
