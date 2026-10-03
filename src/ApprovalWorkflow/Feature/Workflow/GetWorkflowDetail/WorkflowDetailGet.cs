using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workflow.GetWorkflowDetail;

public record WorkflowDetailGetQuery(
    Guid WorkflowId,
    Guid UserId
) : IRequest<WorkflowDetailGetResult>;

public record WorkflowDetailGetResult(
    Guid? WorkflowId,
    Guid? WorkspaceId,
    string? Name,
    string? Description,
    WorkflowStatus? Status,
    List<WorkflowStepDetailGetResult> Steps
);

public record WorkflowStepDetailGetResult(
    Guid? WorkflowStepId,
    string? StepName,
    int StepOrder,
    Guid? ApprovalUserId
);


public class WorkflowDetailGet(
    ISender sender
) : WorkflowApi

{
    [HttpGet("{workflowId:guid}/detail")]
    [ProducesResponseType<ApiResponse<WorkflowDetailGetResult>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workflowId)
    {
        var result = await sender.Send(new WorkflowDetailGetQuery(
            WorkflowId: workflowId,
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<WorkflowDetailGetResult>.Success(
            data: result,
            message: "Success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<WorkflowDetailGetQuery, WorkflowDetailGetResult>
{
    public async Task<WorkflowDetailGetResult> Handle(WorkflowDetailGetQuery req, CancellationToken ct)
    {
        var validWorkflow = await dbContext.WorkFlow.FindAsync(req.WorkflowId, ct)
        ?? throw new WorkflowNotFoundException("Workflow Not Found");

        var validWorkspaceMember = await dbContext.WorkSpaceMember.FirstOrDefaultAsync(
            t => t.WorkSpaceId == validWorkflow.WorkSpaceId
            &&
            t.UserId == req.UserId
            ,ct
        )
        ?? throw new InvalidWorkflowException(reason: "User is not belong to the current Workspace");


        var workflowDetail = await dbContext.WorkFlow.Where(
            t => t.Id == req.WorkflowId
        )
        .Select(t => new WorkflowDetailGetResult(
            WorkflowId: t.Id,
            WorkspaceId: t.WorkSpaceId,
            Name: t.Name,
            Description: t.Description,
            Status: t.Status,
            Steps: t.WorkFlowSteps.OrderBy(wfs => wfs.StepOrder)
            .Select(wfs => new WorkflowStepDetailGetResult(
                WorkflowStepId: wfs.Id,
                StepName: wfs.Name,
                StepOrder: wfs.StepOrder,
                ApprovalUserId: wfs.ApprovedUserId
            ))
            .ToList()
        ))
        .FirstOrDefaultAsync(ct);


        return workflowDetail;
    }
}
