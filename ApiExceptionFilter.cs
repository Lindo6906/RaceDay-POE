using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Filters;

/// <summary>
/// Catches ApiException thrown anywhere in the MVC pipeline and renders the
/// shared ApiError view with the API's message + status code.
/// </summary>
public class ApiExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not ApiException api) return;

        context.Result = new ViewResult
        {
            ViewName = "~/Views/Shared/ApiError.cshtml",
            ViewData =
            {
                ["Message"] = api.Message,
                ["StatusCode"] = (int)api.StatusCode
            }
        };
        context.ExceptionHandled = true;
    }
}