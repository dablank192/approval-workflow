using System;

namespace ApprovalWorkflow.Infrastructure.Persistence.Entities;

public class WorkSpace
{
    public Guid Id {get; set;}
    public required string Name {get; set;}
    public Guid CreatedBy {get; set;} //User - workspace member Id
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}

    public List<WorkSpaceMember> WorkSpaceMember {get; set;}
    public List<WorkFlow> WorkFlow {get; set;}
    public User User {get; set;}
    public List<Requests> Requests {get; set;}
}
