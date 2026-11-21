using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
