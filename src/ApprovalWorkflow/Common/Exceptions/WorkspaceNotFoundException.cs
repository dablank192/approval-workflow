using System;

namespace ApprovalWorkflow.Common.Exceptions;

public class WorkspaceNotFoundException : System.Exception
{
    public string Reason {get;}

    public WorkspaceNotFoundException(string reason) : base(
        message: "Workspace not found"
    )
    {
        Reason = reason;
    }
}
