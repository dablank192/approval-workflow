using System;

namespace ApprovalWorkflow.Feature.Workspace.Exceptions;

public class InvalidRoleAssignmentException : System.Exception
{
    public string Reason {get;}
    public InvalidRoleAssignmentException(string reason) : base(
        message: "Invalid Roles Assignment"
    )
    {
        Reason = reason;
    }
}
