using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workflow.AddWorkflowStep;

public record WorkflowStepAddCommand(
    Guid WorkflowId,
    Guid CurrentUserId,
    Guid ApprovedUserId,
    string Name
) : IRequest<WorkflowStepAddResult>;

public record WorkflowStepAddSubCommand(
    Guid WorkflowId,
    Guid ApprovedUserId,
    string Name
);

public record WorkflowStepAddResult(
    Guid WorkflowId,
    Guid WorkflowStepId,
    int WorkflowStep
);

public class WorkflowStepAdd(
    ISender sender
) : WorkflowApi
{
    [HttpPost("step/new")]
    [ProducesResponseType<ApiResponse<WorkflowStepAddResult>>(statusCode: StatusCodes.Status201Created)]
    public async Task<IActionResult> HandleAsync([FromBody] WorkflowStepAddSubCommand req)
    {
        var result = await sender.Send(new WorkflowStepAddCommand(
            WorkflowId: req.WorkflowId,
            CurrentUserId: User.GetCurrentUserId(),
            ApprovedUserId: req.ApprovedUserId,
            Name: req.Name
        ));

        var response = ApiResponse<WorkflowStepAddResult>.Success(
            data: result,
            message: "Success"
        );

        return StatusCode(
            statusCode: StatusCodes.Status201Created,
            value: response 
        );
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<WorkflowStepAddCommand, WorkflowStepAddResult>
{
    public async Task<WorkflowStepAddResult> Handle(WorkflowStepAddCommand req, CancellationToken ct)
    {
        var validWorkflow = await dbContext.WorkFlow.FirstOrDefaultAsync(
            t => t.Id == req.WorkflowId
            ,ct
        )
        ?? throw new WorkflowNotFoundException(
            reason: "Workflow Id not found"
        );

        if(validWorkflow.Status != WorkflowStatus.Draft) throw new InvalidWorkflowException(
            reason: "Workflow must be Draft"
        );

        var validCurrentUser = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.WorkSpaceId == validWorkflow.WorkSpaceId
            &&
            t.UserId == req.CurrentUserId
            &&
            t.Status == MemberStatus.Active
            ,ct
            );

        if(!validCurrentUser) throw new InvalidWorkflowCreationException(
                reason: "Can't find current member of this Workspace"
            );

        var validApprover = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.UserId == req.ApprovedUserId
            &&
            t.WorkSpaceId == validWorkflow.WorkSpaceId
            &&
            t.Status == MemberStatus.Active
            ,ct
        );

        if(!validApprover) throw new InvalidWorkflowCreationException(
            reason: "Approver must belong in the current Workspace"
        ); 


        var maxStepOrder = await dbContext.WorkFlowSteps.Where(
            t => t.WorkFlowId == req.WorkflowId
        )
        .MaxAsync(t => (int?)t.StepOrder
        ,ct)
        ?? 0;

        var newWfStep = new WorkFlowSteps
        {
            WorkFlowId = req.WorkflowId,
            Name = req.Name,
            StepOrder = maxStepOrder + 1,
            ApprovedUserId = req.ApprovedUserId
        };

        dbContext.WorkFlowSteps.Add(newWfStep);
        await dbContext.SaveChangesAsync(ct);


        return new WorkflowStepAddResult(
            WorkflowId: newWfStep.WorkFlowId,
            WorkflowStepId: newWfStep.Id,
            WorkflowStep: newWfStep.StepOrder
        );
    }
}
