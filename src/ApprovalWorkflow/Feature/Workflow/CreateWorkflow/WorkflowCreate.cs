using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Common.Extensions;
using ApprovalWorkflow.Feature.Workflow.Exceptions;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Workflow.CreateWorkflow;

public record WorkflowCreateCommand(
    Guid WorkspaceId,
    Guid UserId,
    string Name,
    string Description
) : IRequest<WorkflowCreateResult>;

public record WorkflowCreateSubCommand(
    Guid WorkspaceId,
    string Name,
    string Description
);

public record WorkflowCreateResult(
    Guid WorkflowId,
    Guid WorkspaceId
);

public class WorkflowCreate(
    ISender sender
) : WorkflowApi

{
    [HttpPost("new")]
    [ProducesResponseType<ApiResponse<WorkflowCreateResult>>(statusCode: StatusCodes.Status201Created)]
    public async Task<IActionResult> HandleAsync([FromBody] WorkflowCreateSubCommand req)
    {
        var result = await sender.Send(new WorkflowCreateCommand(
            WorkspaceId: req.WorkspaceId,
            UserId: User.GetCurrentUserId(),
            Name: req.Name,
            Description: req.Description
        ));

        var response = ApiResponse<WorkflowCreateResult>.Success(
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
) : IRequestHandler<WorkflowCreateCommand, WorkflowCreateResult>
{
    public async Task<WorkflowCreateResult> Handle (WorkflowCreateCommand req, CancellationToken ct)
    {
        var duplicateWfName = await dbContext.WorkFlow.FirstOrDefaultAsync(
            t => t.Name == req.Name
            &&
            t.WorkSpaceId == req.WorkspaceId
            ,ct
        );
        
        if(duplicateWfName != null) throw new InvalidWorkflowCreationException(
            reason: "Duplicate Workflow name"
        );
        
        var validWf = await dbContext.WorkSpaceMember.AnyAsync(
            t => t.WorkSpaceId == req.WorkspaceId
            &&
            t.UserId == req.UserId
            ,ct
        );

        if(!validWf) throw new InvalidWorkflowCreationException(
            reason: "User is not a member of this Workspace"
        );

        var newWf = new WorkFlow
        {
            WorkSpaceId = req.WorkspaceId,
            Name = req.Name,
            Description = req.Description,
            CreatedBy = req.UserId
        };

        dbContext.WorkFlow.Add(newWf);
        await dbContext.SaveChangesAsync(ct);

        return new WorkflowCreateResult(
            WorkflowId: newWf.Id,
            WorkspaceId: newWf.WorkSpaceId
        );
    }
}
