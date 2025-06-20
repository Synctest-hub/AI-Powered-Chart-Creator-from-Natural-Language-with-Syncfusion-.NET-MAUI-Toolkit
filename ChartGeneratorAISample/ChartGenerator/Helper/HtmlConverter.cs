using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChartGenerator
{
    public class HtmlConverter
    {
        public string ConvertToHTML(string aiResponse)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("<title>AI Response</title>");
            htmlBuilder.AppendLine("<style>");
            htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; line-height:1.6;}");
            htmlBuilder.AppendLine("h1, h2 { color: #33, margin-bottom: 10px; }");
            htmlBuilder.AppendLine("p, li { margin-bottom: 5px;}");
            htmlBuilder.AppendLine("blockquote { border-left: 3px solid #ccc; margin: 10; padding-left: 10px; color: #555; }");
            htmlBuilder.AppendLine("span {display: inline-block; padding: 5px 10px; background-color: #f0f0f0; border-radius: 8px; font-size: 14px; font-family: Consolas, monospace; }");
            htmlBuilder.AppendLine("</style>");
            //htmlBuilder.AppendLine("<script type=\"text/javascript\">");
            //htmlBuilder.AppendLine("function reportHeight() {");
            //htmlBuilder.AppendLine("  var height = document.body.scrollHeight;");
            //htmlBuilder.AppendLine(" window.dispatchEvent(new CustomEvent('reportHeight', { detail: height })); ");
            //htmlBuilder.AppendLine("  window.location.href = 'autofit://' + height;");
            //htmlBuilder.AppendLine("}");
            //htmlBuilder.AppendLine("window.onload = reportHeight;");
            //htmlBuilder.AppendLine("window.onresize = reportHeight;");
            //htmlBuilder.AppendLine("</script>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");

            var paragraphs = aiResponse.Split(new[] { "<br>", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < paragraphs.Count(); i++)
            {
                var paragraph = paragraphs[i];
                if (IsHeading(paragraph))
                {
                    int level = 0;
                    var title = TrimHashFromTitle(paragraph, out level);
                    title = ConvertToHtmlBold(title);
                    if (level == 1)
                        htmlBuilder.AppendLine($"<h1>{title}</h1>");
                    else if (level == 2)
                        htmlBuilder.AppendLine($"<h2>{title}</h2>");
                    else if (level == 3)
                        htmlBuilder.AppendLine($"<h3>{title}</h3>");
                    else if (level == 4)
                        htmlBuilder.AppendLine($"<h4>{title}</h4>");
                    else if (level == 5)
                        htmlBuilder.AppendLine($"<h5>{title}</h5>");
                    else if (level == 6)
                        htmlBuilder.AppendLine($"<h6>{title}</h6>");
                }
                // Implement list parsing here
                // else if (IsList(paragraph))
                // {
                //     ConvertToHtmlList(paragraph, htmlBuilder);
                // }
                else if (IsQuote(paragraph))
                {
                    htmlBuilder.AppendLine($"<blockquote>{paragraph.Trim()}</blockquote>");
                }
                else if (IsURL(paragraph))
                {
                    htmlBuilder.AppendLine($"<a href=\"{paragraph.Trim()}\">{paragraph.Trim()}</a>");
                }
                else if (HasURLText(paragraph))
                {
                    ConvertMarkdownToHtml(paragraph, htmlBuilder);
                }
                else if (ContainsBoldPattern(paragraph))
                {
                    htmlBuilder.AppendLine($"<p>{ConvertToHtmlBold(paragraph)}</p>");
                }
                else if (paragraph.Contains("```"))
                {
                    htmlBuilder.AppendLine("<table style='border: 1px solid black; border-collapse: collapse; width:100%; '>");
                    htmlBuilder.AppendLine("<tr><td>");
                    htmlBuilder.AppendLine("<pre style='margin:10px !important; font-size: 14px; line-height: 1.5; font-family: Consolas, monospace;'>");
                    htmlBuilder.AppendLine("<code>");
                    var codeSnippetIndex = 0;
                    for (codeSnippetIndex = i + 1; codeSnippetIndex < paragraphs.Count(); codeSnippetIndex++)
                    {
                        var codeSnippet = paragraphs[codeSnippetIndex];
                        if (codeSnippet.Contains("```"))
                            break;
                        htmlBuilder.AppendLine(codeSnippet);
                    }
                    htmlBuilder.AppendLine("</code></pre>");
                    htmlBuilder.AppendLine("</td></tr></table>");
                    i = codeSnippetIndex;
                }
                else if (HasBackticks(paragraph))
                {
                    FormatBackTicks(htmlBuilder, paragraph);
                }
                else
                {
                    htmlBuilder.AppendLine($"<p>{paragraph.Trim()}</p>");
                }
            }

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");
            return htmlBuilder.ToString();
        }
        public string TrimHashFromTitle(string title, out int headingLevel)
        {
            headingLevel = 0;

            // Loop from 4 to 1 to check for "####", "###", "##", and "#"
            for (int i = 4; i > 0; i--)
            {
                string hashes = new string('#', i);
                int index = title.IndexOf(hashes);
                if (index != -1)
                {
                    headingLevel = i;
                    // Trim the found hashes from the title and any leading/trailing spaces
                    return title.Substring(0, index).Trim() + title.Substring(index + i).Trim();
                }
            }

            // If no hashes are found, return the original title
            return title;
        }
        private bool IsHeading(string text)
        {
            // Detect headings, e.g., all caps or specific prefixes
            return text == text.ToUpper() || Regex.IsMatch(text, @"^#");
        }

        private bool IsList(string text)
        {
            // Detect bullet points or ordered lists
            return text.StartsWith("- ");// || Regex.IsMatch(text, @"^\d+\.");
        }

        private bool IsQuote(string text)
        {
            // Simplistic quote detection
            return text.StartsWith("\"") && text.EndsWith("\"");
        }

        private bool HasBackticks(string text)
        {
            var backtickPattern = @"`[^`]*`";
            return Regex.IsMatch(text, backtickPattern);
        }

        private void FormatBackTicks(StringBuilder htmlBuilder, string text)
        {
            var backtickPattern = @"`([^`]*)`";
            var formattedText = Regex.Replace(text, backtickPattern, "<span>$1</span>");
            htmlBuilder.AppendLine($"<p>{formattedText}</p>");
        }

        private bool IsURL(string text)
        {
            // URL detection based on simple regex
            var directURLPattern = @"^(http|https):\/\/[^\s$.?#].[^\s]*$";
            return Regex.IsMatch(text, directURLPattern);
        }
        private bool HasURLText(string markdown)
        {
            // Define the regex pattern to extract URLs from Markdown links.
            string pattern = @"\[(.*?)\]\((http[s]?:\/\/[^\s\)]+)\)";

            // Create a match to extract the text and URL from the Markdown link.
            Match match = Regex.Match(markdown, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return IsURL(match.Groups[2].Value);
            }
            return false;
        }
        private static string ConvertMarkdownToHtml(string markdown, StringBuilder htmlBuilder)
        {
            // Define the regex pattern to extract URLs from Markdown links.
            string pattern = @"\[(.*?)\]\((http[s]?:\/\/[^\s\)]+)\)";

            // Create a match to extract the text and URL from the Markdown link.
            Match match = Regex.Match(markdown, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string linkText = match.Groups[1].Value; // Group 1 contains the link text.
                string url = match.Groups[2].Value; // Group 2 contains the URL.        

                htmlBuilder.AppendLine($"<p><a href=\"{url.Trim()}\">{linkText.Trim()}</a></p>");
            }

            // Return original text if no Markdown link is found.
            return markdown;
        }

        private string ConvertToHtmlBold(string input)
        {
            string pattern = @"\*\*(.*?)\*\*";
            string result = Regex.Replace(input, pattern, "<strong>$1</strong>");
            if (HasBackticks(result))
            {
                var backtickPattern = @"`([^`]*)`";
                var formattedText = Regex.Replace(result, backtickPattern, "<span>$1</span>");
                return formattedText;
            }
            return result;
        }

        private bool ContainsBoldPattern(string input)
        {
            string pattern = @"\*\*.+?\*\*";
            return Regex.IsMatch(input, pattern);
        }

        private void ConvertToHtmlList(string paragraph, StringBuilder htmlBuilder)
        {
            var items = paragraph.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            htmlBuilder.AppendLine("<ol>");
            foreach (var item in items)
            {
                htmlBuilder.AppendLine($"<li>{item.Trim().TrimStart('-').Trim()}</li>");
            }
            htmlBuilder.AppendLine("</ol>");
        }
    }
}
