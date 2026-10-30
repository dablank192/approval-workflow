using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workflow.ActivateWorkflow;

public record WorkflowActivateCommand(
    Guid WorkflowId,
    Guid UserId
) : IRequest<WorkflowActivateResult>;

public record WorkflowActivateResult(
    string Message
);

public class WorkflowActivate(
    ISender sender
) : WorkflowApi
{
    [HttpPost("{workflowId:guid}/activate")]
    [ProducesResponseType<ApiResponse<WorkflowActivateResult>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workflowId)
    {
        var result = await sender.Send(new WorkflowActivateCommand(
            WorkflowId: workflowId,
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<WorkflowActivateResult>.Success(
            data: result,
            message: "Success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<WorkflowActivateCommand, WorkflowActivateResult>
{
    public async Task<WorkflowActivateResult> Handle(WorkflowActivateCommand req, CancellationToken ct)
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
            &&
            t.Status == MemberStatus.Active
            ,ct
        )
        ?? throw new UserNotInWorkspaceException(
            reason: "User must be a Workspace Member"
        );

        if(validWf.Status != WorkflowStatus.Draft) throw new InvalidWorkflowStatusException(
            reason: "Workflow status for activation must be in Draft"
        );

        if(validWf.WorkFlowSteps.Count == 0) throw new InvalidWorkflowActivationException(
            reason: "Workflow must have a minimum of step to be activated"
        );


        validWf.Status = WorkflowStatus.Active;

        await dbContext.SaveChangesAsync(ct);

        return new WorkflowActivateResult(
            Message: "Workflow activated"
        );
    }
}
