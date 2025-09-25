using System.Text;

namespace DevExLead.Modules.Confluence.Helpers
{
    public class ConfluenceHelper
    {
        public static string DisplayStatus(string title, string color)
        {
            return $"<ac:structured-macro ac:name=\"status\" ac:schema-version=\"1\" ac:macro-id=\"{Guid.NewGuid()}\"><ac:parameter ac:name=\"title\">{title}</ac:parameter><ac:parameter ac:name=\"colour\">{color}</ac:parameter></ac:structured-macro>";
        }

        public static string DisplayInfoPanel(string text)
        {
            return $"<ac:structured-macro ac:name=\"info\"><ac:parameter ac:name=\"icon\">true</ac:parameter><ac:rich-text-body><b>{text}</b></ac:rich-text-body></ac:structured-macro>";
        }

        public static string DisplayConfluenceToc()
        {
            return $"<ac:structured-macro ac:name=\"toc\" ac:schema-version=\"1\" data-layout=\"default\" ac:local-id=\"{Guid.NewGuid()}\" ac:macro-id=\"{Guid.NewGuid()}\"><ac:parameter ac:name=\"minLevel\">1</ac:parameter><ac:parameter ac:name=\"maxLevel\">6</ac:parameter><ac:parameter ac:name=\"outline\">false</ac:parameter><ac:parameter ac:name=\"type\">list</ac:parameter><ac:parameter ac:name=\"Printable\">false</ac:parameter></ac:structured-macro>";
        }

        public static string DisplayCollapsiblePanel(string title, string htmlContent)
        {
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<ac:structured-macro ac:name=\"expand\">");
            htmlBuilder.AppendLine("  <ac:parameter ac:name=\"title\">Individual Capacity</ac:parameter>");
            htmlBuilder.AppendLine("  <ac:rich-text-body>");
            htmlBuilder.AppendLine(htmlContent);
            htmlBuilder.AppendLine("  </ac:rich-text-body>");
            htmlBuilder.AppendLine("</ac:structured-macro>");
            return htmlBuilder.ToString();
        }
    }
}
