using System;

namespace ApprovalWorkflow.Infrastructure.Persistence.Entities;

public static class MemberRoles
{
    public const string Owner = "O";
    public const string Admin = "A";
    public const string Member = "M";

}

public enum MemberStatus
{
    Active = 0,
    Inactive = 1
}

public class WorkSpaceMember
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public Guid WorkSpaceId {get; set;}
    public required string Roles {get; set;} = MemberRoles.Member;
    public string Status {get; set;} = MemberRoles.Member;
    public DateTime JoinedAt {get; set;}

    public WorkSpace WorkSpace {get; set;}
    public User user {get; set;}
}
