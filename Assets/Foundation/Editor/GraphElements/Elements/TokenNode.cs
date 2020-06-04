using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public class TokenNode : Node
    {
        private Pill m_Pill;

        public Texture icon
        {
            get { return m_Pill.icon; }
            set { m_Pill.icon = value; }
        }

        public Port input
        {
            get { return m_Pill.left as Port; }
        }

        public Port output
        {
            get { return m_Pill.right as Port; }
        }

        public TokenNode(Port input, Port output) : base("TokenNode.uxml")
        {
            this.AddStylesheet("TokenNode.uss");

            m_Pill = this.Q<Pill>(name: "pill");

            if (input != null)
            {
                m_Pill.left = input;
            }

            if (output != null)
            {
                m_Pill.right = output;
            }

            ClearClassList();
            AddToClassList("token-node");
        }

        public bool highlighted
        {
            get { return m_Pill.highlighted; }
            set { m_Pill.highlighted = value; }
        }
    }
}