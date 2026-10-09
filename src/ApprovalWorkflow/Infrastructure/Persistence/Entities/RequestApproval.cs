using System;
using ApprovalWorkflow.Feature.Workflow.AddWorkflowStep;

namespace ApprovalWorkflow.Infrastructure.Persistence.Entities;

public static class ApprovalStatus
{
    public const string Pending = "PEN";
    public const string Approved = "APR";
    public const string Rejected = "REJ";
    public const string Skipped = "SKP";
}

public class RequestApproval
{
    public Guid Id {get; set;}
    public Guid RequestId {get; set;}
    public Guid WorkFlowStepId {get; set;}
    public int StepOrder {get; set;}
    public required string StepName {get; set;}
    public Guid ApproverUserId {get; set;} //user id
    public string Status {get; set;}
    public string? Comment {get; set;}
    public DateTime? ActedAt {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    public Requests Requests {get; set;}
    public WorkFlowSteps WorkFlowSteps {get; set;}
    public User User {get; set;}
}
