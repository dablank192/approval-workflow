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

namespace ApprovalWorkflow.Feature.Workspace.AddMembers;

public record MemberAddCommand(
    Guid UserId,
    Guid NewMemberId,
    Guid WorkspaceId,
    string Roles
) : IRequest<MemberAddResult>;

public record MemberAddSubCommand(
    Guid WorkspaceId,
    Guid NewMemberId,
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
        var result = await sender.Send(new MemberAddCommand(
            UserId: User.GetCurrentUserId(),
            WorkspaceId: req.WorkspaceId,
            NewMemberId: req.NewMemberId,
            Roles: req.Roles
        ));

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
        
        var validUserRoles = await dbContext.WorkSpaceMember
        .FirstOrDefaultAsync(
            t => t.UserId == req.UserId
            &&
            t.WorkSpaceId == req.WorkspaceId
            &&
            t.Status == MemberStatus.Active
            &&
            (t.Roles == MemberRoles.Admin
            || t.Roles == MemberRoles.Owner)
            ,ct
        )
        ?? throw new UserNotFoundException(
            reason: "User's Id not found"
        );

        var memberExisted = await dbContext.User.AnyAsync(t => t.Id == req.NewMemberId, ct);
        
        if (!memberExisted) throw new UserNotFoundException(
            reason: "Member's id not found"
        );

        var validMember = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.UserId == req.NewMemberId
            &&
            t.WorkSpaceId == validUserRoles.WorkSpaceId
            ,ct
        );
        if (validMember) throw new MemberAlreadyExistedException(
            reason: "Member must not belong in the same workspace"
        );

        var newMember = new WorkSpaceMember
        {
            UserId = req.NewMemberId,
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
