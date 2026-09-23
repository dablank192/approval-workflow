using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workspace.CreateWorkspace;
using ApprovalWorkflow.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workspace.GetMyWorkspace;

public record MyWorkspaceGetQuery(
    Guid CurrentUserId
) : IRequest<List<MyWorkSpaceGetResult>>;

public record MyWorkSpaceGetResult(
    Guid? WorkspaceId,
    string? Name,
    string? RoleInWorkspace
);

public class MyWorkspaceGet(
    ISender sender
) : WorkspaceApi
{
    [HttpGet("")]
    [ProducesResponseType<ApiResponse<List<MyWorkSpaceGetResult>>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync()
    {
        var result = await sender.Send(
            new MyWorkspaceGetQuery(
                CurrentUserId: User.GetCurrentUserId()
                ));
        
        var response = ApiResponse<List<MyWorkSpaceGetResult>>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }    
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<MyWorkspaceGetQuery, List<MyWorkSpaceGetResult>>
{
    public async Task<List<MyWorkSpaceGetResult>> Handle(MyWorkspaceGetQuery req, CancellationToken ct)
    {
        var userWp = await dbContext.WorkSpaceMember
        .Where(t => t.UserId == req.CurrentUserId)
        .Select(t => new MyWorkSpaceGetResult(
            WorkspaceId: t.WorkSpace.Id,
            Name: t.WorkSpace.Name,
            RoleInWorkspace: t.Roles
        ))
        .ToListAsync(ct);

        return userWp;
    }
}
