using System;

namespace ApprovalWorkflow.Feature.Workflow.Exceptions;

public class InvalidWorkflowStatusException : System.Exception
{
    public string Reason {get;}

    public InvalidWorkflowStatusException(string reason) : base(
        message: "Workflow's status is invalid"
    )
    {
        Reason = reason;
    }
}
