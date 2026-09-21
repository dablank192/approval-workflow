using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Workspace.GetWorkspace;

public record WorkspaceGetQuery(
    Guid WorkspaceId
) : IRequest<WorkspaceGetResult>;

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
    public async Task<IActionResult> HandleAsync([AsParameters] WorkspaceGetQuery req)
    {
        var result = await sender.Send(req);

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
        var wp = await dbContext.WorkSpace.FindAsync(req.WorkspaceId, ct);
        
        var response = new WorkspaceGetResult(
            WorkspaceId: wp.Id,
            Name: wp.Name,
            CreatedBy: wp.CreatedBy,
            CreatedAt: wp.CreatedAt,
            UpdatedAt: wp.UpdatedAt
        );

        return response;
    }
}
