using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Request.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Request.CancelRequest;

public record RequestCancelCommand(
    Guid RequestId,
    Guid CurrentUserId
) : IRequest<RequestCancelResult>;

public record RequestCancelResult(
    Guid? RequestId,
    string? RequestStatus,
    DateTime? CompletedAt
);

public class RequestCancel(
    ISender sender
) : RequestApi
{
    [HttpPost("{requestId:guid}/cancel")]
    public async Task<IActionResult> HandleAsync(Guid requestId)
    {
        var result = await sender.Send(new RequestCancelCommand(
            RequestId: requestId,
            CurrentUserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<RequestCancelResult>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<RequestCancelCommand, RequestCancelResult>
{
    public async Task<RequestCancelResult> Handle(RequestCancelCommand req, CancellationToken ct)
    {
        var validReq = await dbContext.Requests.FirstOrDefaultAsync(
            t => t.Id == req.RequestId
            &&
            t.CreatedBy == req.CurrentUserId
            &&
            dbContext.WorkSpaceMember.Any(
                u => u.WorkSpaceId == t.WorkSpaceId
                &&
                u.UserId == req.CurrentUserId
                &&
                u.Status == MemberStatus.Active)
            ,ct
        )
        ?? throw new InvalidOwnerOrApproverException(
            reason: "Current user and request must belong in the same Workspace"
        );

        if(!(validReq.Status == RequestStatus.Draft
        ||
        validReq.Status == RequestStatus.Pending))
        throw new InvalidRequestStatusException(
            reason: "Request must be Draft or Pending to be Cancel"
        );

        validReq.Status = RequestStatus.Cancelled;
        validReq.CompletedAt = DateTime.UtcNow;
        validReq.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);


        return new RequestCancelResult(
            RequestId: validReq.Id,
            RequestStatus: validReq.Status,
            CompletedAt: validReq.CompletedAt
        );
    }
}
