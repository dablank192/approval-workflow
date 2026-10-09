using System;
using System.Net;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Exceptions;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workspace.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Request.CreateRequest;

public record RequestCreateCommand(
    Guid WorkspaceId,
    Guid WorkflowId,
    Guid CreatedBy,
    string Title,
    string Description
) : IRequest<RequestCreateResult>;

public record RequestCreateSubCommand(
    Guid WorkspaceId,
    Guid WorkflowId,
    string Title,
    string Description
);

public record RequestCreateResult(
    Guid RequestId,
    string Message
);

public class RequestCreate(
    ISender sender
) : RequestApi
{
    [HttpPost("")]
    [ProducesResponseType<ApiResponse<RequestCreateResult>>(statusCode: StatusCodes.Status201Created)]
    public async Task<IActionResult> HandleAsync([FromBody] RequestCreateCommand req)
    {
        var result = await sender.Send(new RequestCreateCommand(
            WorkflowId: req.WorkflowId,
            WorkspaceId: req.WorkspaceId,
            CreatedBy: User.GetCurrentUserId(),
            Title: req.Title,
            Description: req.Description
        ));

        var response = ApiResponse<RequestCreateResult>.Success(
            data: result,
            message: "Success"
        );

        return StatusCode(
            statusCode: StatusCodes.Status201Created,
            value: response
        );
    }
}


/// <summary>
/// Create Request
/// </summary>
/// <remarks>
/// Request status on creation is "Default",
/// and can only be change to "Pending" if request be submitted
/// </remarks>
public class Handler(
    AppDbContext dbContext
) : IRequestHandler<RequestCreateCommand, RequestCreateResult>
{
    public async Task<RequestCreateResult> Handle(RequestCreateCommand req, CancellationToken ct)
    {
        //Check if user is a Workspace Member
        var validWpMember = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.UserId == req.CreatedBy
            &&
            t.WorkSpaceId == req.WorkspaceId
            ,ct
        );
        if(!validWpMember) throw new UserNotInWorkspaceException(
            reason: "User not a member of Workspace"
        );

        var validWorkflow = await dbContext.WorkFlow.FirstOrDefaultAsync(
            t => t.Id == req.WorkflowId
            ,ct
        )
        ?? throw new WorkflowNotFoundException(
            reason: "Workflow's Id not found or not existed"
        );

        if(validWorkflow.WorkSpaceId != req.WorkspaceId) throw new WorkflowNotFoundException(
            reason: "Workflow not found or not existed in current Workspace"
        );


        var newRequest = new Requests
        {
            WorkFlowId = req.WorkflowId,
            WorkSpaceId = req.WorkspaceId,
            CreatedBy = req.CreatedBy,
            Title = req.Title,
            Description = req.Description,
            Status = RequestStatus.Draft //status on creation
        };

        dbContext.Requests.Add(newRequest);
        await dbContext.SaveChangesAsync(ct);

        
        return new RequestCreateResult(
            RequestId: newRequest.Id,
            Message: "Request created successfully"
        );
    }
}
