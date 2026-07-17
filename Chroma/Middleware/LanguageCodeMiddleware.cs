using System.Globalization;

namespace Chroma.Middleware;

public sealed class LanguageCodeMiddleware(RequestDelegate next)
{
    public const string HeaderName = "LangCode";
    public const string ContextItemName = "LangCode";

    public async Task InvokeAsync(HttpContext context)
    {
        var requested = context.Request.Headers[HeaderName].FirstOrDefault();
        var langCode = string.Equals(requested, "tr", StringComparison.OrdinalIgnoreCase)
            ? "tr"
            : "en";

        context.Items[ContextItemName] = langCode;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(langCode);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(langCode);

        await next(context);
    }

    public static string GetLanguage(HttpContext context)
        => context.Items[ContextItemName]?.ToString() ?? "en";
}
