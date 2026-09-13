using System;

namespace ApprovalWorkflow.Infrastructure.Persistence.Entities;

public class User
{
    public Guid Id {get; set;}
    public required string Email {get; set;}
    public required string Password {get; set;}
    public required string DisplayName {get; set;}
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}

    public List<WorkSpaceMember> WorkspaceMember {get; set;}
    public List<WorkFlowSteps> WorkFlowSteps {get; set;}
    public List <WorkFlow> WorkFlow {get; set;}
    public List<Requests> Requests {get; set;}
    public List<RequestApproval> RequestApproval {get; set;}
    public List<WorkSpace> WorkSpace {get; set;}
}
