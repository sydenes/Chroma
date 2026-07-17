using Chroma.Application.Common.Responses;
using Chroma.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chroma.Localization;

public sealed class ApiResponseLocalizationFilter(IApiMessageLocalizer localizer) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: ApiResponse response }
            && !string.IsNullOrWhiteSpace(response.MessageCode)
            && !string.IsNullOrWhiteSpace(response.Message))
        {
            response.Message = localizer.Localize(
                response.MessageCode,
                response.Message,
                LanguageCodeMiddleware.GetLanguage(context.HttpContext));
        }

        await next();
    }
}
