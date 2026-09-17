using System;

namespace ApprovalWorkflow.Infrastructure.Persistence.Entities;

public class WorkFlowSteps
{
    public Guid Id {get; set;}
    public Guid WorkFlowId {get; set;}
    public required string Name {get; set;}
    public int StepOrder {get; set;}
    public Guid ApprovedUserId {get; set;} //user id
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    public WorkFlow WorkFlow {get; set;}
    public User User {get; set;}
}
