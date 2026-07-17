using System.Text.Json;

namespace Chroma.Localization;

public sealed class JsonApiMessageLocalizer(IWebHostEnvironment environment) : IApiMessageLocalizer
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _messages =
        LoadMessages(environment.ContentRootPath);

    public string Localize(string? messageCode, string fallbackMessage, string? langCode = null)
    {
        if (string.IsNullOrWhiteSpace(messageCode))
        {
            return fallbackMessage;
        }

        var language = NormalizeLanguage(langCode);
        return _messages.TryGetValue(language, out var translations)
            && translations.TryGetValue(messageCode, out var translated)
            && !string.IsNullOrWhiteSpace(translated)
                ? translated
                : fallbackMessage;
    }

    private static string NormalizeLanguage(string? langCode)
        => string.Equals(langCode, "tr", StringComparison.OrdinalIgnoreCase) ? "tr" : "en";

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadMessages(
        string contentRootPath)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var language in new[] { "en", "tr" })
        {
            var path = Path.Combine(contentRootPath, "Localization", $"messages.{language}.json");
            if (!File.Exists(path))
            {
                result[language] = new Dictionary<string, string>();
                continue;
            }

            var json = File.ReadAllText(path);
            result[language] = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
        }

        return result;
    }
}
