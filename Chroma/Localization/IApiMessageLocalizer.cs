namespace Chroma.Localization;

public interface IApiMessageLocalizer
{
    string Localize(string? messageCode, string fallbackMessage, string? langCode = null);
}
