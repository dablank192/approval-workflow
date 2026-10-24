using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Request.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Request.GetRequestDetail;

public record RequestDetailGetQuery(
    Guid RequestId,
    Guid CurrentUserId
) : IRequest<RequestDetailGetResult>;

public record RequestDetailGetResult(
    Guid? RequestId,
    Guid? WorkspaceId,
    Guid? WorkflowId,
    Guid? CreatedBy,
    string? Title,
    string? Description,
    string? Status,
    DateTime? SubmittedAt,
    DateTime? CompletedAt,
    List<ApprovalHistory> ApprovalHistories
);

public record ApprovalHistory(
    Guid? RequestApprovalId,
    int? StepOrder,
    string? StepName,
    Guid? ApproverUserId,
    string? Status,
    string? Comment,
    DateTime? ActedAt
);

public class RequestDetailGet(
    ISender sender
) : RequestApi
{
    [HttpGet("{requestId:guid}/detail")]
    [ProducesResponseType<ApiResponse<RequestDetailGetResult>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync(Guid requestId)
    {
        var result = await sender.Send(new RequestDetailGetQuery(
            RequestId: requestId,
            CurrentUserId: User.GetCurrentUserId()
        ));

        var response = ApiResponse<RequestDetailGetResult>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<RequestDetailGetQuery, RequestDetailGetResult>
{
    public async Task<RequestDetailGetResult> Handle(RequestDetailGetQuery req, CancellationToken ct)
    {
        var validRequest = await dbContext.Requests
        .FirstOrDefaultAsync(
            t => t.Id == req.RequestId
            ,ct
        )
        ?? throw new RequestNotFoundException(
            reason: "Request's Id not found"
            );

        var validWpMb = await dbContext.WorkSpaceMember
        .AnyAsync(
            t => t.UserId == req.CurrentUserId
            &&
            t.Status == MemberStatus.Active
            &&
            t.WorkSpaceId == validRequest.WorkSpaceId
            ,ct
        );

        if(!validWpMb) throw new InvalidRequestAccessException(
            reason: "User must be an Active Workspace member and Request must belong in a Workspace"
        );

        
        //check whether user is a request owner or approver 
        if(!((validRequest.CreatedBy == req.CurrentUserId) // check if user is a creator
        &&
        await dbContext.RequestApproval.AnyAsync( // check if user is a approver
            t => t.RequestId == validRequest.Id
            &&
            t.ApproverUserId == req.CurrentUserId
            ,ct
        ))
            ) throw new InvalidOwnerOrApproverException(
            reason: "Only Request's owner or Request's approver can view the Request Detail"
        );


        var apiResponse = await dbContext.Requests
        .Where(t => t.Id == req.RequestId)
        .Select(
            t => new RequestDetailGetResult(
                RequestId: t.Id,
                WorkspaceId: t.WorkSpaceId,
                WorkflowId: t.WorkFlowId,
                CreatedBy: t.CreatedBy,
                Title: t.Title,
                Description: t.Description,
                Status: t.Status,
                SubmittedAt: t.SubmittedAt,
                CompletedAt: t.CompletedAt,
                ApprovalHistories: t.RequestApproval
                .OrderBy(u => u.StepOrder)
                .Select(u => new ApprovalHistory(
                    RequestApprovalId: u.Id,
                    StepOrder: u.StepOrder,
                    StepName: u.StepName,
                    ApproverUserId: u.ApproverUserId,
                    Status: u.Status,
                    Comment: u.Comment,
                    ActedAt: u.ActedAt
                ))
                .ToList()
            )
        )
        .FirstOrDefaultAsync(ct);


        return apiResponse;
    }
}
