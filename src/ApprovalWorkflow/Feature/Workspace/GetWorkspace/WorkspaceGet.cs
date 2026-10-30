using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workspace.GetWorkspace;

public record WorkspaceGetQuery(
    Guid WorkspaceId,
    Guid UserId
) : IRequest<WorkspaceGetResult>;

public record WorkspaceGetSubQuery(
    Guid WorkspaceId
);

public record WorkspaceGetResult(
    Guid? WorkspaceId,
    string? Name,
    Guid? CreatedBy,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);

public class WorkspaceGet(
    ISender sender
) : WorkspaceApi
{
    [HttpGet("{workspaceId}")]
    [ProducesResponseType<WorkspaceGetResult>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([AsParameters] WorkspaceGetSubQuery req)
    {
        var result = await sender.Send(new WorkspaceGetQuery(
            WorkspaceId: req.WorkspaceId,
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<WorkspaceGetResult>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<WorkspaceGetQuery, WorkspaceGetResult>
{
    public async Task<WorkspaceGetResult> Handle(WorkspaceGetQuery req, CancellationToken ct)
    {
        var wp = await dbContext.WorkSpaceMember.FirstOrDefaultAsync(
            t => t.WorkSpaceId == req.WorkspaceId
            &&
            t.UserId == req.UserId
            , ct);
        
        var response = new WorkspaceGetResult(
            WorkspaceId: wp.WorkSpaceId,
            Name: wp.WorkSpace.Name,
            CreatedBy: wp.WorkSpace.CreatedBy,
            CreatedAt: wp.WorkSpace.CreatedAt,
            UpdatedAt: wp.WorkSpace.UpdatedAt
        );

        return response;
    }
}
