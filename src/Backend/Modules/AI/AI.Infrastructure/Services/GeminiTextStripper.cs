using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Marketing.Domain.Enums;

namespace Marketing.Infrastructure.Services;

// ── Strip Helper ─────────────────────────────────────────────────────────────

public static class GeminiTextStripper
{
    public static string StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Strip HTML tags
        var result = Regex.Replace(input, "<.*?>", string.Empty, RegexOptions.Singleline);

        // Strip markdown bold/italic (**text** or *text*)
        result = Regex.Replace(result, @"\*{1,2}(.*?)\*{1,2}", "$1");

        // Strip markdown links [text](url) → text (e.g. Instagram hashtag links)
        result = Regex.Replace(result, @"\[([^\]]+)\]\([^\)]+\)", "$1");

        return result.Trim();
    }

    public static string BodyBlocksToPlainText(string? bodyJson, ExternalPlatform platform, Guid id)
    {
        if (string.IsNullOrWhiteSpace(bodyJson)) return string.Empty;

        var trimmed = bodyJson.Trim();

        // Case 1: double-serialized quoted string → unwrap outer quotes first
        // e.g. body stored as "\"[{...}]\"" instead of "[{...}]"
        if (trimmed.StartsWith('"'))
        {
            try { trimmed = JsonSerializer.Deserialize<string>(trimmed) ?? trimmed; }
            catch { /* use as-is */ }
        }

        // Case 3: body is a raw JSON object instead of array (LLM bug) → wrap it
        if (trimmed.StartsWith('{'))
            trimmed = $"[{trimmed}]";

        // Try parsing as-is first (handles normal JSON with escaped quotes inside string values)
        JsonElement[]? blocks = TryParseBlocks(trimmed);

        // Case 2: only unescape \" if the normal parse failed.
        // Do NOT blindly replace \" → " on the raw string, because \" is valid JSON
        // inside string values (e.g. "text": "He said \"hello\"") — replacing it breaks
        // the JSON structure and causes Deserialize to throw, falling back to plain StripHtml.
        if (blocks is null && trimmed.Contains("\\\""))
        {
            var unescaped = trimmed.Replace("\\\"", "\"");
            blocks = TryParseBlocks(unescaped);
        }

        if (blocks is null)
        {
            // Not a block array at all — fall back to plain HTML/markdown strip
            return StripHtml(bodyJson);
        }

        var sb = new StringBuilder();

        foreach (var block in blocks)
        {
            var type = block.TryGetProperty("type", out var t) ? t.GetString() : null;

            switch (type)
            {
                case "heading":
                    var level = block.TryGetProperty("level", out var l) ? l.GetInt32() : 1;
                    var headTxt = block.TryGetProperty("text", out var ht) ? ht.GetString() : "";
                    sb.AppendLine($"{new string('#', level)} {StripHtml(headTxt)}");
                    break;

                case "paragraph":
                    var paraTxt = block.TryGetProperty("text", out var pt) ? pt.GetString() : "";
                    sb.AppendLine(StripHtml(paraTxt));
                    break;

                case "highlight":
                    var hlTxt = block.TryGetProperty("content", out var hc) ? hc.GetString() : "";
                    sb.AppendLine($"★ {StripHtml(hlTxt)}");
                    break;

                case "list":
                    var ordered = block.TryGetProperty("ordered", out var o) && o.GetBoolean();
                    if (block.TryGetProperty("items", out var items))
                    {
                        int idx = 1;
                        foreach (var item in items.EnumerateArray())
                        {
                            var bullet = ordered ? $"{idx++}." : "-";
                            sb.AppendLine($"{bullet} {StripHtml(item.GetString())}");
                        }
                    }
                    break;

                case "button":
                    var label = block.TryGetProperty("label", out var lb) ? lb.GetString() : "";
                    var href = block.TryGetProperty("href", out var hr) ? hr.GetString() : "";
                    if (!string.IsNullOrEmpty(href))
                    {
                        var platformCode = platform switch
                        {
                            ExternalPlatform.Facebook => "fb",
                            ExternalPlatform.Instagram => "ig",
                            ExternalPlatform.Threads => "th",
                            _ => platform.ToString().ToLower()
                        };
                        href = $"{href}?ref={id}&p={platformCode}";
                    }
                    sb.AppendLine($"[{StripHtml(label)}] → {href}");
                    break;

                case "image":
                    var alt = block.TryGetProperty("alt", out var a) ? a.GetString() : "";
                    var src = block.TryGetProperty("src", out var s) ? s.GetString() : "";
                    sb.AppendLine($"[Image: {alt}] ({src})");
                    break;

                case "divider":
                    sb.AppendLine("────────────");
                    break;

                default:
                    if (block.TryGetProperty("text", out var fallback))
                        sb.AppendLine(StripHtml(fallback.GetString()));
                    else if (block.TryGetProperty("content", out var fallback2))
                        sb.AppendLine(StripHtml(fallback2.GetString()));
                    break;
            }

            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Attempts to deserialize a JSON string as a block array.
    /// Returns null (instead of throwing) on any parse failure.
    /// </summary>
    private static JsonElement[]? TryParseBlocks(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<JsonElement[]>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
