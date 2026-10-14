using System;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Feature.Approval.Exceptions;
using ApprovalWorkflow.Feature.Auth.Exceptions;
using ApprovalWorkflow.Feature.Request.Exceptions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
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

            InvalidRoleAssignmentException =>(
                StatusCodes.Status403Forbidden,
                "Invalid Roles Assignment"
            ),

            UserNotFoundException => (
                StatusCodes.Status404NotFound,
                "User Not Found"
            ),

            InvalidWorkflowCreationException => (
                StatusCodes.Status400BadRequest,
                "Invalid Workflow Creation"
            ),

            InvalidWorkflowException => (
                StatusCodes.Status400BadRequest,
                "Invalid Workflow"
            ),

            WorkflowNotFoundException => (
                StatusCodes.Status404NotFound,
                "Workflow not found"
            ),

            UserNotInWorkspaceException => (
                StatusCodes.Status400BadRequest,
                "User not belong in the Workspace"
            ),

            InvalidWorkflowStatusException => (
                StatusCodes.Status400BadRequest,
                "Invalid Workflow status"
            ),

            InvalidWorkflowActivationException => (
                StatusCodes.Status406NotAcceptable,
                "Invalid Workflow Activation"
            ),

            InvalidRequestStatusException => (
                StatusCodes.Status422UnprocessableEntity,
                "Request status invalid"
            ),
            
            RequestNotFoundException => (
                StatusCodes.Status404NotFound,
                "Request Not Found"
            ),

            WorkspaceNotFoundException => (
                StatusCodes.Status404NotFound,
                "Workspace not found"
            ),

            ApproverNotFoundException => (
                StatusCodes.Status404NotFound,
                "Approver not found"
            ),
            
            InvalidReqApprovalStatusException => (
                StatusCodes.Status400BadRequest,
                "Approval Status is invalid"
            ),

            InvalidApproverException => (
                StatusCodes.Status404NotFound,
                "Approver not found"
            ),

            InvalidStepOrderException => (
                StatusCodes.Status409Conflict,
                "Step's Order invalid"
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
