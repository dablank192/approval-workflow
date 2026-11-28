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
using Npgsql.Internal;

namespace ApprovalWorkflow.Feature.User.GetUser;

public record UserGetQuery(
    Guid WorkspaceId,
    Guid CurrentUserId,
    Guid? UserId,
    string? UserEmail
) : IRequest<UserGetResult>;

public record UserGetSubQuery(
    Guid WorkspaceId,
    Guid? UserId,
    string? UserEmail
);

public record UserGetResult(
    Guid UserId,
    string DisplayName,
    string Email,
    bool IsWorkspaceMember
);

public class UserGet(
    ISender sender
) : UserApi
{
    [HttpGet("lookup")]
    [ProducesResponseType<ApiResponse<UserGetResult>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromQuery] UserGetSubQuery req)
    {
        var result = await sender.Send(new UserGetQuery(
            WorkspaceId: req.WorkspaceId,
            CurrentUserId: User.GetCurrentUserId(),
            UserId: req.UserId,
            UserEmail: req.UserEmail
        ));

        var response = ApiResponse<UserGetResult>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<UserGetQuery, UserGetResult>
{
    public async Task<UserGetResult> Handle(UserGetQuery req, CancellationToken ct)
    {
        var validWp = await dbContext.WorkSpace.AnyAsync(
            t => t.Id == req.WorkspaceId
            ,ct
        );
        if(validWp == false) throw new WorkspaceNotFoundException(
            reason: "Workspace not found or not existed"
        ); //throw 200 empty if none

        var validWpMember = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.WorkSpaceId == req.WorkspaceId
            && t.UserId == req.CurrentUserId
            && t.Status == MemberStatus.Active
            && (t.Roles == MemberRoles.Owner || t.Roles == MemberRoles.Admin)
            ,ct
        );
        if(validWpMember == false) throw new UserNotInWorkspaceException(
            reason: "Member not found or not existed in workspace"
        ); //throw 200 empty if none

        var validUser = await dbContext.User.FirstOrDefaultAsync(
            t => t.Id == req.UserId
            || t.Email == req.UserEmail
            ,ct
        );

        var isValidUserWpMember = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.UserId == validUser.Id
            ,ct
        );

        var response = new UserGetResult(
            UserId: validUser.Id,
            DisplayName: validUser.DisplayName,
            Email: validUser.Email,
            IsWorkspaceMember: isValidUserWpMember
        );

        return response;
    }
}
