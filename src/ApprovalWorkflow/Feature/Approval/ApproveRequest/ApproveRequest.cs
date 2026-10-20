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

namespace ApprovalWorkflow.Feature.Approval.ApproveRequest;

public record ApproveRequestCommand(
    Guid RequestApprovalId,
    Guid UserId,
    string? Comment = null
) : IRequest<ApproveRequestResult>;

public record ApproveRequestSubCommand(
    string? Comment = null
);

public record ApproveRequestResult(
    Guid RequestId,
    Guid RequestApprovalId,
    string Message
);

public class ApproveRequest(
    ISender sender
) : ApprovalApi
{
    [HttpPost("{reqApprId:guid}/approve")]
    [ProducesResponseType<ApproveRequestResult>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromRoute]Guid reqApprId, [FromBody] ApproveRequestSubCommand body)
    {
        var result = await sender.Send(new ApproveRequestCommand(
            RequestApprovalId: reqApprId,
            Comment: body.Comment,
            UserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<ApproveRequestResult>.Success(
            data: result,
            message: "Success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<ApproveRequestCommand, ApproveRequestResult>
{
    public async Task<ApproveRequestResult> Handle(ApproveRequestCommand req, CancellationToken ct)
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


        validRequestApr.Status = ApprovalStatus.Approved;
        validRequestApr.ActedAt = DateTime.UtcNow;

        var isAllApproved = validRequest.RequestApproval
        .All(
            t => t.Status == ApprovalStatus.Approved
        );

        if (isAllApproved)
        {
            validRequest.Status = RequestStatus.Approved;
            validRequest.CompletedAt = DateTime.UtcNow;
        }

        validRequest.UpdatedAt = DateTime.UtcNow;

        validRequestApr.Comment = req.Comment;

        await dbContext.SaveChangesAsync(ct);


        return new ApproveRequestResult(
            RequestId: validRequest.Id,
            RequestApprovalId: validRequestApr.Id,
            Message: "Step Approved successfully!"
        );
    }
}
