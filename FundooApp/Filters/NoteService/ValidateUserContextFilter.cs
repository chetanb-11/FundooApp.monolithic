using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BuildingBlocks.Common.Models;

namespace NoteService.Api.Filters;

public class ValidateUserContextFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sub = context.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!int.TryParse(sub, out var userId) || userId <= 0)
        {
            context.Result = new UnauthorizedObjectResult(new ResponseModel<string>
            {
                Success = false,
                Message = "Invalid user claims in token"
            });
            return;
        }

        context.HttpContext.Items["userId"] = userId;
        await next();
    }
}