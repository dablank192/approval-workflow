using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workflow.GetWorkflow;

public record WorkflowGetQuery(
    Guid WorkspaceId,
    Guid UserId
) : IRequest<List<WorkflowGetResult>>;

public record WorkflowGetResult(
    Guid? WorkflowId,
    Guid? WorkspaceId,
    string? WorkflowName,
    string? Description,
    WorkflowStatus? Status,
    int? StepCount
);

public class WorkflowGet(
    ISender sender
) : WorkflowApi
{
    [HttpGet("{workspaceId}")]
    [ProducesResponseType<ApiResponse<List<WorkflowGetResult>>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid workspaceId)
    {
        var result = await sender.Send(new WorkflowGetQuery(
            WorkspaceId: workspaceId,
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<List<WorkflowGetResult>>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<WorkflowGetQuery, List<WorkflowGetResult>>
{
    public async Task<List<WorkflowGetResult>> Handle(WorkflowGetQuery req, CancellationToken ct)
    {
        var validWp = await dbContext.WorkSpace.AnyAsync(t => t.Id == req.WorkspaceId, ct);

        if(!validWp) throw new WorkspaceNotFoundException(
            reason: "Workspace's Id not existed or workspace's Id invalid"
        );

        var validMemberQuery = dbContext.WorkSpaceMember
        .Where(
            t => t.WorkSpaceId == req.WorkspaceId
            &&
            t.UserId == req.UserId
            &&
            t.Status == MemberStatus.Active
        );

        var validWpMember = await validMemberQuery.AnyAsync(ct);

        if(!validWpMember) throw new UserNotInWorkspaceException(
            reason: "User's status must be active and belong in the Workspace"
        );

        var validWf = await dbContext.WorkFlow
        .Where(
            t => t.WorkSpaceId == req.WorkspaceId
            &&
            validMemberQuery.Any()
        )
        .Select(
            t => new WorkflowGetResult(
                WorkflowId: t.Id,
                WorkspaceId: t.WorkSpaceId,
                WorkflowName: t.Name,
                Description: t.Description,
                Status: t.Status,
                StepCount: t.WorkFlowSteps.Count
            )
        )
        .ToListAsync(ct);

        return validWf;
    }
}