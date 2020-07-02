using System;
using System.Text;

namespace Modifier.DotsStencil
{
    class MarkdownNodeDocumentationFormatter : NodeDocumentationFormatter
    {
        readonly StringBuilder m_StringBuilder = new StringBuilder();

        public override string ToString()
        {
            return m_StringBuilder.ToString();
        }

        protected override void Paragraph(string paragraph)
        {
            m_StringBuilder.Append($"{paragraph}\n\n");
        }

        protected override void SectionTitle(string title, int titleImportance)
        {
            m_StringBuilder.Append($"{new string('#', titleImportance)} {title}\n\n");
        }

        protected override void PortDescription(string portName, string type, string defaultValue, string attrDescription)
        {
            m_StringBuilder.Append($"**{portName}**|_{type}_|{defaultValue ?? String.Empty}|{attrDescription}\n");
        }

        protected override void PortsHeader(string sectionName)
        {
            SectionTitle(sectionName, 3);
            m_StringBuilder.Append($"Port Name|Type|Default Value|Description\n---|---|---|---\n");
        }
    }
}
