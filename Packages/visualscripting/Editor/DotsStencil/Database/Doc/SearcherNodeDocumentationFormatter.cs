using UnityEngine.UIElements;

namespace Modifier.DotsStencil
{
    class SearcherNodeDocumentationFormatter : NodeDocumentationFormatter
    {
        private readonly VisualElement m_Root;

        public SearcherNodeDocumentationFormatter(VisualElement root)
        {
            m_Root = root;
        }

        protected override void Paragraph(string paragraph)
        {
            var label = new Label(paragraph);
            label.AddToClassList("paragraph");
            m_Root.Add(label);
        }

        protected override void SectionTitle(string title, int titleImportance)
        {
            var label = new Label(title);
            label.AddToClassList("title");
            label.AddToClassList("title-" + titleImportance);
            m_Root.Add(label);
        }

        protected override void PortDescription(string portName, string type, string defaultValue, string attrDescription)
        {
            SectionTitle(portName, 4);
            Paragraph("Type:" + type);
            if (defaultValue != null)
                Paragraph("Default Value: " + defaultValue);
            if (attrDescription != null)
                Paragraph(attrDescription);
        }

        protected override void PortsHeader(string sectionName)
        {
            SectionTitle(sectionName, 3);
        }
    }
}
