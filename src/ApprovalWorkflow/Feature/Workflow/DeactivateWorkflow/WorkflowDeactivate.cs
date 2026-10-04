using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workflow.DeactivateWorkflow;

public record WorkflowDeactivateCommand(
    Guid WorkflowId,
    Guid UserId
) : IRequest<WorkflowDeactivateResult>;

public record WorkflowDeactivateResult(
    string Message
);

public class WorkflowDeactivate(
    ISender sender
) : WorkflowApi
{
    [HttpPost("{workflowId:guid}/deactive")]
    [ProducesResponseType<ApiResponse<WorkflowDeactivateResult>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workflowId)
    {
        var result = await sender.Send(new WorkflowDeactivateCommand(
            WorkflowId: workflowId,
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<WorkflowDeactivateResult>.Success(
            data: result,
            message: "Success"
        );

        return Ok(response);
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<WorkflowDeactivateCommand, WorkflowDeactivateResult>
{
    public async Task<WorkflowDeactivateResult> Handle(WorkflowDeactivateCommand req, CancellationToken ct)
    {
        var validWf = await dbContext.WorkFlow
        .Include(t => t.WorkFlowSteps)
        .FirstOrDefaultAsync(
            t => t.Id == req.WorkflowId
            , ct
        )
        ?? throw new WorkflowNotFoundException(
            reason: "Workflow's Id not found"
        );

        var validWpMember = await dbContext.WorkSpaceMember.FirstOrDefaultAsync(
            t => t.UserId == req.UserId
            &&
            t.WorkSpaceId == validWf.WorkSpaceId
            ,ct
        )
        ?? throw new UserNotInWorkspaceException(
            reason: "User must be a Workspace Member"
        );

        if(validWf.Status != WorkflowStatus.Active) throw new InvalidWorkflowStatusException(
            reason: "Workflow status for deactivation must be in Active"
        );

        validWf.Status = WorkflowStatus.Inactive;
        await dbContext.SaveChangesAsync(ct);


        return new WorkflowDeactivateResult(
            Message: "Workflow Deactivated"
        );
    }
}
