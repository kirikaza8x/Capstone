using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Application.Helpers;

public static partial class SlugHelper
{
    public static string Generate(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return WhitespaceRegex()
            .Replace(
                NonAlphanumericRegex()
                    .Replace(sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant(), "-"),
                "-")
            .Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex WhitespaceRegex();
}