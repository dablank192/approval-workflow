using System;
using System.Net;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workspace.AddMembers;

public record MemberAddCommand(
    Guid UserId,
    Guid WorkspaceId,
    string Roles
) : IRequest<MemberAddResult>;

public record MemberAddSubCommand(
    Guid WorkspaceId,
    string Roles
);

public record MemberAddResult(
    Guid? UserId
);

public class MemberAdd(
    ISender sender
) : WorkspaceApi
{
    [HttpPost("new-member")]
    [ProducesResponseType<ApiResponse<MemberAddResult>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> HandleAsync([FromBody] MemberAddSubCommand req, CancellationToken ct)
    {
        var result = await sender.Send(
            new MemberAddCommand(
                UserId: User.GetCurrentUserId(),
                WorkspaceId: req.WorkspaceId,
                Roles: req.Roles
            ), ct);

        var response = ApiResponse<MemberAddResult>.Success(
            data: result,
            message: "Member added successfully"
        );

        return StatusCode(
            statusCode: StatusCodes.Status201Created,
            value: response
        );
    }
}

public class Handler(
    AppDbContext dbContext
): IRequestHandler<MemberAddCommand, MemberAddResult>
{
    public async Task<MemberAddResult> Handle(MemberAddCommand req, CancellationToken ct)
    {
        if(req.Roles == MemberRoles.Owner) throw new InvalidRoleAssignmentException(
            reason: "Owner role aren't allowed while adding new member"
        );
        
        var user = await dbContext.User.FindAsync(req.UserId, ct)
        ?? throw new UserNotFoundException(
            reason: "User's Id not found"
        );

        var newMember = new WorkSpaceMember
        {
            UserId = req.UserId,
            WorkSpaceId = req.WorkspaceId,
            Roles = req.Roles
        };

        dbContext.WorkSpaceMember.Add(newMember);
        await dbContext.SaveChangesAsync(ct);

        return new MemberAddResult(
            UserId: newMember.UserId
        );
    }
}
