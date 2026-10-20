using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Approval.Exceptions;
using ApprovalWorkflow.Feature.Request.Exceptions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Approval.RejectRequest;

public record RejectRequestCommand(
    Guid RequestApprovalId,
    Guid UserId,
    string Comment
) : IRequest<RejectRequestResult>;

public record RejectRequestSubCommand(
    Guid RequestApprovalId,
    string Comment
);

public record RejectRequestResult(
    Guid RequestId,
    Guid RequestApprovalId,
    string Message
);

public class RejectRequest(
    ISender sender
) : ApprovalApi
{
    [HttpPost("reject")]
    [ProducesResponseType<RejectRequestResult>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromBody] RejectRequestSubCommand req)
    {
        var result = await sender.Send(new RejectRequestCommand(
            RequestApprovalId: req.RequestApprovalId,
            UserId: User.GetCurrentUserId(),
            Comment: req.Comment
        ));

        var response = ApiResponse<RejectRequestResult>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<RejectRequestCommand, RejectRequestResult>
{
    public async Task<RejectRequestResult> Handle(RejectRequestCommand req, CancellationToken ct)
    {
        //Check if Request Id and Request approval id existed
        var validRequestApr = await dbContext.RequestApproval
        .FirstOrDefaultAsync(
            t => t.Id == req.RequestApprovalId
            &&
            dbContext.Requests.Any(
                u => u.Id == t.RequestId
            )
            ,ct
        )
        ?? throw new ApproverNotFoundException(
            reason: "Invalid Request Approval record or Request's Id not found"
        );

        if(validRequestApr.ApproverUserId != req.UserId) throw new InvalidApproverException(
            reason: "Approver Id must be current user id"
        );
        if(validRequestApr.Status != ApprovalStatus.Pending) throw new InvalidReqApprovalStatusException(
            reason: "Request approval status must be Pending"
        );


        //Check if Request existed and is Pending
        var validRequest = await dbContext.Requests
        .Include(t => t.RequestApproval)
        .FirstOrDefaultAsync(
            t => t.Id == validRequestApr.RequestId
            &&
            t.Status == RequestStatus.Pending
            ,ct
        )
        ?? throw new InvalidRequestStatusException(
            reason: "Request's Id not found or request status is not Pending"
        );


        var validPrevReq = await dbContext.RequestApproval.AnyAsync(
            t => t.RequestId == validRequestApr.RequestId
            &&
            t.Status == ApprovalStatus.Pending
            &&
            t.StepOrder < validRequestApr.StepOrder
            ,ct
        );
        if(validPrevReq) throw new InvalidStepOrderException(
            reason: "Current step must be the latest Pending step"
        );

        var validWpMember = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.WorkSpaceId == validRequest.WorkSpaceId
            &&
            t.UserId == req.UserId
            &&
            t.Status == MemberStatus.Active
            ,ct
        );
        if(!validWpMember) throw new UserNotInWorkspaceException(
            reason: "User's Id must be Active and belong to current Workspace"
        );


        validRequestApr.Status = ApprovalStatus.Rejected;
        validRequestApr.ActedAt = DateTime.UtcNow;
        validRequest.Status = RequestStatus.Rejected;
        validRequest.CompletedAt = DateTime.UtcNow;
        validRequestApr.Comment = req.Comment;

        await dbContext.SaveChangesAsync(ct);

        return new RejectRequestResult(
            RequestId: validRequest.Id,
            RequestApprovalId: validRequestApr.Id,
            Message: "Step reject successfully"
        );
    }
}
