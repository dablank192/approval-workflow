using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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
    [ProducesResponseType<Result>(statusCode: StatusCodes.Status201Created)]
    [HttpPost("new", Name = "CreateWorkspace")]
    public async Task<IActionResult> HandleAsync([FromBody] SubCommand req, CancellationToken ct)
    {   
        var result = await sender.Send(new Command(
            WorkspaceName: req.WorkspaceName,
            UserId: User.GetCurrentUserId()
        ),ct);

        return CreatedAtRoute(
            routeName: "CreateWorkspace",
            value: result,
            routeValues: null
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

        var workSpace = await dbContext.WorkSpace.FindAsync(newWs.Id, ct);
        
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
