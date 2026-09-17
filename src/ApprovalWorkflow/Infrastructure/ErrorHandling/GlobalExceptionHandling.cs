using System;
using ApprovalWorkflow.Feature.Auth.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Infrastructure.ErrorHandling;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        logger.LogError(
            message: exception.Message,
            args: exception
        );

        var (statusCode, detailMessage) = exception switch
        {
            InvalidUsernameOrPasswordException =>(
                StatusCodes.Status401Unauthorized,
                "Invalid Username or Password"
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Unknown System Error"
            )
        };


        httpContext.Response.StatusCode = statusCode;

        var problemDetail = new ProblemDetails
        {
            Status = statusCode,
            Detail = detailMessage,
            Instance = httpContext.Request.Path
        };

        if (environment.IsDevelopment())
        {
            problemDetail.Extensions.Add("SystemDetail", exception.Message);
            problemDetail.Extensions.Add("Traceback", exception.StackTrace);
        }

        await httpContext.Response.WriteAsJsonAsync(
            problemDetail,
            ct
        );

        return true;
    }
}
