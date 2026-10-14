using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Security;

public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string Role = "Role";
}

/// <summary>
/// Session-based authorisation. Returns 401 when nobody is logged in and
/// 403 when the logged-in user's role is not the one required.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole? _role;

    public SessionAuthorizeAttribute() { }
    public SessionAuthorizeAttribute(UserRole role) { _role = role; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (session.GetInt32(SessionKeys.UserId) is null)
        {
            context.Result = new ObjectResult(new { message = "You must be logged in to access this resource." })
            { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        if (_role is not null && session.GetString(SessionKeys.Role) != _role.ToString())
        {
            context.Result = new ObjectResult(new { message = $"This action is restricted to the {_role} role." })
            { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
