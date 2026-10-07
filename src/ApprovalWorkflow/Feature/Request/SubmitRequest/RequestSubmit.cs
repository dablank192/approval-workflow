using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Request.Exceptions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Request.SubmitRequest;

public record RequestSubmitCommand(
    Guid UserId,
    Guid RequestId
) : IRequest<RequestSubmitResult>;

public record RequestSubmitResult(
    Guid RequestId,
    string RequestStatus,
    DateTime? SubmittedAt,
    int ApprovalStepCount
);

public class RequestSubmit(
    ISender sender
) : RequestApi
{
    [HttpPost("{requestId}/submit")]
    [ProducesResponseType<ApiResponse<RequestSubmitResult>>(statusCode: StatusCodes.Status201Created)]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid requestId)
    {
        var result = await sender.Send(new RequestSubmitCommand(
            UserId: User.GetCurrentUserId(),
            RequestId: requestId
        ));

        var response = ApiResponse<RequestSubmitResult>.Success(
            data: result,
            message: "Success"
        );

        return StatusCode(
            statusCode: StatusCodes.Status201Created,
            value: response
        );
    }
}


public class Handler(
    AppDbContext dbContext
) : IRequestHandler<RequestSubmitCommand, RequestSubmitResult>
{
    public async Task<RequestSubmitResult> Handle(RequestSubmitCommand req, CancellationToken ct)
    {
        var validRequest = await dbContext.Requests.FirstOrDefaultAsync(
            t => t.Id == req.RequestId
            &&
            t.CreatedBy == req.UserId
            ,ct
        )
        ?? throw new RequestNotFoundException(
            reason: "Request Id not existed or not found"
        );

        if(validRequest.Status != RequestStatus.Draft) throw new InvalidRequestStatusException(
            reason: "Request status before submitted must be Draft"
        );

        var validWp = await dbContext.WorkSpace.FirstOrDefaultAsync(
            t => t.Id == validRequest.WorkSpaceId
            ,ct
        )
        ?? throw new WorkspaceNotFoundException(
            reason: "Workspace Id from Request record is invalid or not found"
        );

        var validWpMember = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.WorkSpaceId == validWp.Id
            &&
            t.UserId == req.UserId
            ,ct
        );
        if(!validWpMember) throw new UserNotInWorkspaceException(
            reason: "User is not a member of current Workspace"
        );

        var validWf = await dbContext.WorkFlow
        .Include(t => t.WorkFlowSteps)
        .FirstOrDefaultAsync(
            t => t.Id == validRequest.WorkFlowId
            &&
            t.WorkSpaceId == validWp.Id
            ,ct
        )
        ?? throw new InvalidWorkflowException(
            reason: "Workflow must existed and in the same Workspace as the Request"
        );

        if (validWf.Status != WorkflowStatus.Active) throw new InvalidWorkflowStatusException(
            reason: "Workflow status must be active"
        );

        var totalWfStep = validWf.WorkFlowSteps.Count;

        if (totalWfStep < 1) throw new InvalidWorkflowException(
            reason: "Workflow step minimum must be 1, in order for request to submit"
        );

        foreach(var step in validWf.WorkFlowSteps.OrderBy(t => t.StepOrder))
        {
            var approval = new RequestApproval
            {
                RequestId = req.RequestId,
                WorkFlowStepId = step.Id,
                StepOrder = step.StepOrder,
                StepName = step.Name,
                ApproverUserId = step.ApprovedUserId,
                Status = ApprovalStatus.Pending
            };

            dbContext.RequestApproval.Add(approval);
        }

        validRequest.Status = RequestStatus.Pending;
        validRequest.SubmittedAt = DateTime.UtcNow;
        validRequest.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);


        return new RequestSubmitResult(
            RequestId: validRequest.Id,
            RequestStatus: validRequest.Status,
            SubmittedAt: validRequest.SubmittedAt,
            ApprovalStepCount: totalWfStep
        );
    }
}