using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Workspace.CreateWorkspace;

public record Command(
    string WorkspaceName,
    Guid UserId
) : IRequest<Result>;

public record SubCommand(
    string WorkspaceName
);

public record Result(
    Guid WorkspaceId
);

public class WorkspaceCreate(
    ISender sender
) : WorkspaceApi
{
    [ProducesResponseType<ApiResponse<Result>>(statusCode: StatusCodes.Status201Created)]
    [HttpPost("new", Name = "CreateWorkspace")]
    public async Task<IActionResult> HandleAsync([FromBody] SubCommand req, CancellationToken ct)
    {   
        var result = await sender.Send(new Command(
            WorkspaceName: req.WorkspaceName,
            UserId: User.GetCurrentUserId()
        ), ct);

        var response = ApiResponse<Result>.Success(
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
) : IRequestHandler<Command, Result>

{
    public async Task<Result> Handle(Command req, CancellationToken ct)
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

        return new Result(
            WorkspaceId: newWs.Id
        );
    }
}
