using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Request.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Request.GetRequest;

public record RequestGetQuery(
    Guid CurrentUserId
) : IRequest<List<RequestGetResult>>;

public record RequestGetResult(
    Guid? RequestId,
    Guid? WorkspaceId,
    Guid? WorkflowId,
    string? Title,
    string? Status,
    DateTime? CreatedAt,
    DateTime? SubmittedAt,
    DateTime? CompletedAt
);

public class RequestGet(
    ISender sender
) : RequestApi
{
    [HttpGet("me")]
    [ProducesResponseType<ApiResponse<List<RequestGetResult>>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync()
    {
        var result = await sender.Send(new RequestGetQuery(CurrentUserId: User.GetCurrentUserId()));

        var response = ApiResponse<List<RequestGetResult>>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<RequestGetQuery, List<RequestGetResult>>
{
    public async Task<List<RequestGetResult>> Handle (RequestGetQuery req, CancellationToken ct)
    {
        var validRequest = await dbContext.Requests
        .FirstOrDefaultAsync(
            t => t.CreatedBy == req.CurrentUserId
            ,ct
        );

        var validWpMember = await dbContext.WorkSpaceMember.FirstOrDefaultAsync(
            t => t.WorkSpaceId == validRequest.WorkSpaceId
            &&
            t.UserId == req.CurrentUserId
            ,ct
        )
        ?? throw new InvalidOwnerOrApproverException(
            reason: "Both Request and User must belong to the same Workspace"
        );

        var response = await dbContext.Requests.Where(
            t => t.CreatedBy == req.CurrentUserId
            &&
            t.WorkSpaceId == validWpMember.WorkSpaceId
        )
        .Select(t => new RequestGetResult(
            RequestId: t.Id,
            WorkspaceId: t.WorkSpaceId,
            WorkflowId: t.WorkFlowId,
            Title: t.Title,
            Status: t.Status,
            CreatedAt: t.CreatedAt,
            SubmittedAt: t.SubmittedAt,
            CompletedAt: t.CompletedAt
        ))
        .ToListAsync(ct);

        return response;
    }
}
