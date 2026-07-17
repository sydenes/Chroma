using Chroma.Application.Common.Responses;
using Chroma.Localization;
using Chroma.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chroma.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(string permission) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (!context.HttpContext.User.HasClaim("permission", permission))
        {
            const string fallback = "You do not have permission to perform this action.";
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IApiMessageLocalizer>();
            var message = localizer.Localize(
                "auth.forbidden",
                fallback,
                LanguageCodeMiddleware.GetLanguage(context.HttpContext));

            context.Result = new ObjectResult(ApiResponse.Fail("auth.forbidden", message))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
