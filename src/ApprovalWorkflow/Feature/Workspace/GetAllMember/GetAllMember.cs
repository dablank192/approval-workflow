using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workspace.GetAllMember;

public record GetAllMemberQuery(
    Guid WorkspaceId,
    string? Roles
) : IRequest<List<GetAllMemberResult>>;

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
    public async Task<IActionResult> HandleAsync([FromQuery] GetAllMemberQuery req)
    {
        var result = await sender.Send(req);

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
