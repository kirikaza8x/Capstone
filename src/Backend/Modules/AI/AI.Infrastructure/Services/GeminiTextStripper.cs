using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Marketing.Application.Services;
using Marketing.Domain.Entities;
using Marketing.Domain.Enums;
using Marketing.Infrastructure.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marketing.Infrastructure.Services;

// ── Strip Helper ─────────────────────────────────────────────────────────────

public static class GeminiTextStripper
{
    public static string StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input, "<.*?>", string.Empty, RegexOptions.Singleline).Trim();
    }

    public static string BodyBlocksToPlainText(string? bodyJson)
    {
        if (string.IsNullOrWhiteSpace(bodyJson)) return string.Empty;

        var blocks = JsonSerializer.Deserialize<JsonElement[]>(bodyJson);
        if (blocks is null) return string.Empty;

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
                    sb.AppendLine($"[{StripHtml(label)}] → {href}");
                    break;

                case "image":
                    var alt = block.TryGetProperty("alt", out var a) ? a.GetString() : "";
                    var src = block.TryGetProperty("src", out var s) ? s.GetString() : "";
                    sb.AppendLine($"[Image: {alt}] ({src})");
                    break;

                case "divider":
                    sb.AppendLine("────────────────────────────────");
                    break;

                default:
                    if (block.TryGetProperty("text", out var fallback))
                        sb.AppendLine(StripHtml(fallback.GetString()));
                    break;
            }

            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }
}

