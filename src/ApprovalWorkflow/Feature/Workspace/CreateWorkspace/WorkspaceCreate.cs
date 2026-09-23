using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Workspace.CreateWorkspace;

public record WorkspaceCreateCommand(
    string WorkspaceName,
    Guid UserId
) : IRequest<WorkspaceCreateResult>;

public record SubWorkspaceCreateCommand(
    string WorkspaceName
);

public record WorkspaceCreateResult(
    Guid WorkspaceId
);

public class WorkspaceCreate(
    ISender sender
) : WorkspaceApi
{
    [ProducesResponseType<ApiResponse<WorkspaceCreateResult>>(statusCode: StatusCodes.Status201Created)]
    [HttpPost("new", Name = "CreateWorkspace")]
    public async Task<IActionResult> HandleAsync([FromBody] SubWorkspaceCreateCommand req, CancellationToken ct)
    {   
        var result = await sender.Send(new WorkspaceCreateCommand(
            WorkspaceName: req.WorkspaceName,
            UserId: User.GetCurrentUserId()
        ), ct);

        var response = ApiResponse<WorkspaceCreateResult>.Success(
            data: result,
            message: "Workspace created successfully"
        );

        return StatusCode(
            statusCode: StatusCodes.Status201Created,
            value: response
        );
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<WorkspaceCreateCommand, WorkspaceCreateResult>

{
    public async Task<WorkspaceCreateResult> Handle(WorkspaceCreateCommand req, CancellationToken ct)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        
        var newWs = new WorkSpace
        {
            Name = req.WorkspaceName,
            CreatedBy = req.UserId
        };
        
        dbContext.WorkSpace.Add(newWs);
        await dbContext.SaveChangesAsync(ct);
        
        var newMember = new WorkSpaceMember
        {
            UserId = req.UserId,
            WorkSpaceId = newWs.Id,
            Roles = MemberRoles.Owner            
        };

        dbContext.WorkSpaceMember.Add(newMember);
        await dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return new WorkspaceCreateResult(
            WorkspaceId: newWs.Id
        );
    }
}
