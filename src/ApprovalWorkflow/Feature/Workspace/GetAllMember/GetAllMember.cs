using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workspace.GetAllMember;

public record GetAllMemberQuery(
    Guid WorkspaceId,
    string? Roles,
    Guid UserId
) : IRequest<List<GetAllMemberResult>>;

public record GetAllMemberSubQuery(
    Guid WorkspaceId,
    string? Roles
);

public record GetAllMemberResult(
    Guid UserId,
    string Roles,
    MemberStatus Status,
    DateTime JoinedAt
);

public class GetAllMember(
    ISender sender
) : WorkspaceApi
{
    [HttpGet("members")]
    [ProducesResponseType<List<GetAllMemberResult>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromQuery] GetAllMemberSubQuery req)
    {
        var result = await sender.Send(new GetAllMemberQuery(
            WorkspaceId: req.WorkspaceId,
            Roles: req.Roles,
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<List<GetAllMemberResult>>.Success(
            data: result,
            message: "Success"
        );

        return Ok(response);
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<GetAllMemberQuery, List<GetAllMemberResult>>
{
    public async Task<List<GetAllMemberResult>> Handle(GetAllMemberQuery req, CancellationToken ct)
    {
        var validUser = await dbContext.WorkSpaceMember.FirstOrDefaultAsync(
            t => t.UserId == req.UserId
            &&
            t.WorkSpaceId == req.WorkspaceId
            &&
            t.Status == MemberStatus.Active,
            ct
        )
        ?? throw new UserNotFoundException(reason: "User is not the owner of this workspace");
        
        var members = await dbContext.WorkSpaceMember
        .Where(t => t.WorkSpaceId == req.WorkspaceId)
        .Select(t => new GetAllMemberResult(
            UserId: t.UserId,
            Roles: t.Roles,
            Status: t.Status,
            JoinedAt: t.JoinedAt
        ))
        .ToListAsync(ct);

        return members;
    }
}
