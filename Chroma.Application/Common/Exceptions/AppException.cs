namespace Chroma.Application.Common.Exceptions;

/// <summary>
/// User-facing application error with a stable localization code and English fallback message.
/// </summary>
public sealed class AppException(
    string messageCode,
    string fallbackMessage,
    int statusCode = 409) : Exception(fallbackMessage)
{
    public string MessageCode { get; } = messageCode;
    public int StatusCode { get; } = statusCode;
}
