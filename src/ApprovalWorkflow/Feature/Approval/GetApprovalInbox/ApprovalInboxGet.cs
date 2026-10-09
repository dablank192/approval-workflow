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
        var reqApproval = await dbContext.RequestApproval
        .Include(t => t.Requests)
        .FirstOrDefaultAsync(
            t => t.ApproverUserId == req.UserId
            ,ct
        )
        ?? throw new ApproverNotFoundException(
            reason: "Approver's UserId not found or not existed"
        );

        if(reqApproval.Status != ApprovalStatus.Pending) throw new InvalidReqApprovalStatusException(
            reason: "Current user Approval Status is invalid"
        );

        if(reqApproval.Requests.Status != RequestStatus.Pending) throw new InvalidRequestStatusException(
            reason: "Current ticket's status must be Pending"
        );


        var wfMember = await dbContext.WorkSpaceMember.FirstOrDefaultAsync(
            t => t.UserId == req.UserId
            ,ct
        ) ?? throw new UserNotInWorkspaceException(
            reason: "User not a member of current Workspace"
        );

        var validWf = await dbContext.WorkFlow.FirstOrDefaultAsync(
            t => t.Id == reqApproval.Requests.WorkFlowId
            ,ct
        ) ?? throw new InvalidWorkflowException(
            reason: "Workflow must be existed/active in current Workspace"
        );
    }
}
