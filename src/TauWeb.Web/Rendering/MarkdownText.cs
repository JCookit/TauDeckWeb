using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace TauWeb.Web.Rendering;

public static class MarkdownText
{
    private static readonly Regex ParagraphBreak = new(@"(?:\r?\n){2,}", RegexOptions.Compiled);

    public static string Inline(string text, Func<string, string?>? linkUrl = null)
    {
        return RenderInline(text ?? "", 0, linkUrl);
    }

    public static string Blocks(string text, Func<string, string?>? linkUrl = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        var paragraphs = ParagraphBreak.Split(text.Trim())
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .Select(paragraph => $"<p>{RenderInline(paragraph.Trim().ReplaceLineEndings("\n"), 0, linkUrl).Replace("\n", "<br />")}</p>");

        return string.Join("", paragraphs);
    }

    private static string RenderInline(string text, int depth, Func<string, string?>? linkUrl)
    {
        if (depth > 8 || text.Length == 0)
        {
            return HtmlEncoder.Default.Encode(text);
        }

        var output = new StringBuilder();
        for (var index = 0; index < text.Length;)
        {
            if (TryRenderStrong(text, ref index, depth, output, linkUrl)
                || TryRenderEmphasis(text, ref index, depth, output, linkUrl)
                || TryRenderLink(text, ref index, depth, output, linkUrl))
            {
                continue;
            }

            output.Append(HtmlEncoder.Default.Encode(text[index].ToString()));
            index++;
        }

        return output.ToString();
    }

    private static bool TryRenderStrong(string text, ref int index, int depth, StringBuilder output, Func<string, string?>? linkUrl)
    {
        if (!text[index..].StartsWith("**", StringComparison.Ordinal))
        {
            return false;
        }

        var close = text.IndexOf("**", index + 2, StringComparison.Ordinal);
        if (close < 0)
        {
            return false;
        }

        var inner = text[(index + 2)..close];
        output.Append("<strong>").Append(RenderInline(inner, depth + 1, linkUrl)).Append("</strong>");
        index = close + 2;
        return true;
    }

    private static bool TryRenderEmphasis(string text, ref int index, int depth, StringBuilder output, Func<string, string?>? linkUrl)
    {
        if (text[index] != '*' || text[index..].StartsWith("**", StringComparison.Ordinal))
        {
            return false;
        }

        var close = text.IndexOf('*', index + 1);
        if (close < 0)
        {
            return false;
        }

        var inner = text[(index + 1)..close];
        output.Append("<em>").Append(RenderInline(inner, depth + 1, linkUrl)).Append("</em>");
        index = close + 1;
        return true;
    }

    private static bool TryRenderLink(string text, ref int index, int depth, StringBuilder output, Func<string, string?>? linkUrl)
    {
        if (text[index] != '[')
        {
            return false;
        }

        var labelEnd = text.IndexOf(']', index + 1);
        if (labelEnd < 0 || labelEnd + 1 >= text.Length || text[labelEnd + 1] != '(')
        {
            return false;
        }

        var hrefEnd = text.IndexOf(')', labelEnd + 2);
        if (hrefEnd < 0)
        {
            return false;
        }

        var label = text[(index + 1)..labelEnd];
        var href = text[(labelEnd + 2)..hrefEnd].Trim();
        if (label.Length == 0 || !IsAllowedLocalHref(href))
        {
            return false;
        }

        var url = linkUrl is null ? href : linkUrl(href);
        var renderedLabel = RenderInline(label, depth + 1, linkUrl);
        if (url is null)
            output.Append(renderedLabel);
        else
            output.Append("<a href=\"").Append(HtmlEncoder.Default.Encode(url)).Append("\">").Append(renderedLabel).Append("</a>");
        index = hrefEnd + 1;
        return true;
    }

    private static bool IsAllowedLocalHref(string href)
    {
        return href.StartsWith('#')
            || href.StartsWith('/') && !href.StartsWith("//", StringComparison.Ordinal);
    }
}
