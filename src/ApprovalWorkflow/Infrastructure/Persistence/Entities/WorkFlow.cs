using System;

namespace ApprovalWorkflow.Infrastructure.Persistence.Entities;

public enum WorkflowStatus
{
    Draft = 0,
    Active = 1,
    Inactive = 2
}

public class WorkFlow
{
    public Guid Id {get; set;}
    public Guid WorkSpaceId {get; set;}
    public required string Name {get; set;}
    public string? Description {get; set;}
    public WorkflowStatus Status {get; set;} = WorkflowStatus.Draft;
    public Guid CreatedBy {get; set;} //User id
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    public WorkSpace WorkSpace {get; set;}
    public List<WorkFlowSteps> WorkFlowSteps {get; set;}
    public List<Requests> Requests {get; set;}
    public User User {get; set;}
    public List<RequestApproval> RequestApproval {get; set;}
}
