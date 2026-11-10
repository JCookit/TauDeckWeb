using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TauWeb.Core.Import;

public static partial class Slug
{
    public static string Create(string value, string fallback = "item")
    {
        value = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }

        var slug = DuplicateDashRegex().Replace(builder.ToString(), "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? fallback : slug;
    }

    [GeneratedRegex("-+")]
    private static partial Regex DuplicateDashRegex();
}
