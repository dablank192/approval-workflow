using System;

namespace ApprovalWorkflow.Feature.Workflow.Exceptions;

public class InvalidWorkflowException : System.Exception
{
    public string Reason {get;}

    public InvalidWorkflowException(string reason) : base(
        message: "Invalid Workflow"
    )
    {
        Reason = reason;
    }
}
