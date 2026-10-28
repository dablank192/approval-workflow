using System;

namespace ApprovalWorkflow.Feature.Workspace.Exceptions;

public class MemberAlreadyExistedException : System.Exception
{
    public string Reason {get;}

    public MemberAlreadyExistedException(string reason) : base(
        message: "Member already existed"
    )
    {
        Reason = reason;
    }
}
