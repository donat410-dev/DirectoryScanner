using System.Text;

namespace DirectoryScanner
{
    public static class WriteToHtml
    {
        public static void Start(FileSystemElement element, string outputPath = "index.html")
        {
            var htmlContent = new StringBuilder();
            htmlContent.Append("<html><head><style>");
            htmlContent.Append("body { font-family: Arial, sans-serif; background-color: #2E2E2E; color: #E0E0E0; }");
            htmlContent.Append(".folder { color: #6495ED; }");
            htmlContent.Append(".file { color: #FF69B4; }");
            htmlContent.Append(".size { color: #00CED1; }");
            htmlContent.Append(".mimetype { color: #7FFF00; }");
            htmlContent.Append("</style></head><body>");
            htmlContent.Append("<h1>Файловая структура</h1>");

            BuildFileManager(element, htmlContent);

            htmlContent.Append("</body></html>");
            File.WriteAllText(outputPath, htmlContent.ToString());
        }

        private static void BuildFileManager(FileSystemElement element, StringBuilder htmlContent, string indent = "")
        {
            htmlContent.Append("<div>");
            htmlContent.Append($"{indent}|- ");
            htmlContent.Append($"<span class=\"folder\">{element.Name}</span>");
            htmlContent.Append($"&ensp;<span class=\"size\">({FileSystemElement.FormatFileSize(element.Size)})</span>");
            htmlContent.Append("<br>");

            indent += "|&ensp;";
            foreach (var item in element.ChildrenElements)
            {
                if (item.MimeType == "folder/folder")
                {
                    BuildFileManager(item, htmlContent, indent);
                }
                else
                {
                    htmlContent.Append($"{indent}|- ");
                    htmlContent.Append($"<span class=\"file\">{item.Name}</span>");
                    htmlContent.Append($"&ensp;<span class=\"size\">({FileSystemElement.FormatFileSize(item.Size)})</span>&ensp;");
                    htmlContent.Append($"<span class=\"mimetype\">{item.MimeType}</span>");
                    htmlContent.Append("<br>");
                }
            }

            htmlContent.Append("</div>");
        }
    }
}
