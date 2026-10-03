using System;

namespace ApprovalWorkflow.Common.Exceptions;

public class WorkflowNotFoundException : System.Exception
{
    public string Reason {get;}

    public WorkflowNotFoundException(string reason) : base(
        "Workflow Id is incorrecct"
    )
    {
        Reason = reason;
    }
}
