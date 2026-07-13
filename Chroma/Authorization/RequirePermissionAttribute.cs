using Chroma.Application.Common.Responses;
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
            context.Result = new ObjectResult(ApiResponse.Fail("You do not have permission to perform this action."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
