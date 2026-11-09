using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters;

/// <summary>
/// Redirects to /Account/Login when the Web session has no user id,
/// and to /Account/Denied when the role doesn't match.
/// The API is still the real authority — this is just a friendly UX wrapper.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class WebSessionAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string? _role;
    public WebSessionAuthorizeAttribute() { }
    public WebSessionAuthorizeAttribute(string role) => _role = role;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("WebUserId");

        if (userId is null)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult(
                "Login", "Account",
                new { returnUrl = returnUrl.ToString() });
            return;
        }

        if (_role is not null && session.GetString("WebRole") != _role)
        {
            context.Result = new RedirectToActionResult("Denied", "Account", null);
        }
    }
}