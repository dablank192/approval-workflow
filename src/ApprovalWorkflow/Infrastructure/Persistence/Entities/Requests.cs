using System;

namespace ApprovalWorkflow.Infrastructure.Persistence.Entities;

public static class RequestStatus
{
    public const string Draft = "DA";
    public const string Pending = "PED";
    public const string Approved = "APR";
    public const string Rejected = "RJE";
    public const string Cancelled = "CCD";
}

public class Requests
{
    public Guid Id {get; set;}
    public Guid WorkSpaceId {get; set;}
    public Guid WorkFlowId {get; set;}
    public Guid CreatedBy {get; set;}
    public required string Title {get; set;}
    public string? Description {get; set;}
    public string Status {get; set;} 
    public DateTime SubmittedAt {get; set;}
    public DateTime CompletedAt {get; set;}
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}

    public WorkFlow WorkFlow {get; set;}
    public WorkSpace WorkSpace {get; set;}
    public User User {get; set;}
    public List<RequestApproval> RequestApproval {get; set;}
}
