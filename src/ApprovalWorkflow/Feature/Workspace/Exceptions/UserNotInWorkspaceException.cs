using System;

namespace ApprovalWorkflow.Feature.Workspace.Exceptions;

public class UserNotInWorkspaceException : System.Exception
{
    public string Reason {get;}

    public UserNotInWorkspaceException(string reason) : base(
        message: "User is not a member of this workspace"
    )
    {
        Reason = reason;
    }
}
