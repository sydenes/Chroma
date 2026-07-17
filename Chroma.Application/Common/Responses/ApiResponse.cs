namespace Chroma.Application.Common.Responses;

public class ApiResponse
{
    public bool Success { get; init; }
    public string? MessageCode { get; init; }
    public string? Message { get; set; }

    public static ApiResponse Ok(string? message = null) => new() { Success = true, Message = message };
    public static ApiResponse Ok(string messageCode, string fallbackMessage)
        => new() { Success = true, MessageCode = messageCode, Message = fallbackMessage };
    public static ApiResponse Fail(string message)
        => new() { Success = false, MessageCode = "common.error", Message = message };
    public static ApiResponse Fail(string messageCode, string fallbackMessage)
        => new() { Success = false, MessageCode = messageCode, Message = fallbackMessage };
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Ok(T data, string messageCode, string fallbackMessage)
        => new() { Success = true, Data = data, MessageCode = messageCode, Message = fallbackMessage };
}
