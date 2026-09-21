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
    public string Roles {get; set;} = MemberRoles.Member;
    public MemberStatus Status {get; set;} = MemberStatus.Active;
    public DateTime JoinedAt {get; set;} = DateTime.UtcNow;

    public WorkSpace WorkSpace {get; set;}
    public User User {get; set;}
}
