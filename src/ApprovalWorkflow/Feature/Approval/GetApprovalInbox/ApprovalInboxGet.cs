using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Approval.Exceptions;
using ApprovalWorkflow.Feature.Request.Exceptions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Approval.GetApprovalInbox;

public record ApprovalInboxGetQuery(
    Guid UserId
) : IRequest<List<ApprovalInboxGetResult>>;

public record ApprovalInboxGetResult(
    Guid? RequestApprovalId,
    Guid? RequestId,
    Guid? WorkspaceId,
    string? RequestTitle,
    Guid? RequestBy,
    string? StepName,
    int? StepOrder,
    DateTime? SubmittedAt
);

public class ApprovalInboxGet(
    ISender sender
) : ApprovalApi
{
    [HttpGet("{userId}/inbox")]
    public async Task<IActionResult> HandleAsync()
    {
        var result = await sender.Send(new ApprovalInboxGetQuery(
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<List<ApprovalInboxGetResult>>.Success(
            data: result,
            message: "Success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<ApprovalInboxGetQuery, List<ApprovalInboxGetResult>>
{
    public async Task<List<ApprovalInboxGetResult>> Handle(ApprovalInboxGetQuery req, CancellationToken ct)
    {
        var approvalInbox = await dbContext.RequestApproval
        .AsNoTracking()
        .Where(
            t => t.Status == ApprovalStatus.Pending
            &&
            t.Requests.Status == RequestStatus.Pending

            //Check current step
            && !dbContext.RequestApproval.Any(
                u => u.RequestId == t.RequestId
                &&
                u.Status == ApprovalStatus.Pending
                &&
                u.StepOrder < t.StepOrder
            )

            //Check workspace membership
            && dbContext.WorkSpaceMember.Any(
                mb => mb.UserId == req.UserId
                &&
                mb.WorkSpaceId == t.Requests.WorkSpaceId
                &&
                mb.Status == MemberStatus.Active
            )
        )
        .Select(t => new ApprovalInboxGetResult(
            RequestApprovalId: t.Id,
            RequestId: t.RequestId,
            WorkspaceId: t.Requests.WorkSpaceId,
            RequestTitle: t.Requests.Title,
            RequestBy: t.Requests.CreatedBy,
            StepName: t.StepName,
            StepOrder: t.StepOrder,
            SubmittedAt: t.Requests.SubmittedAt
        ))
        .ToListAsync(ct);

        return approvalInbox;
    }
}
