using System;

namespace ApprovalWorkflow.Feature.Workflow.Exceptions;

public class InvalidWorkflowDeactivationException : System.Exception
{
    public string Reason {get;}

    public InvalidWorkflowDeactivationException (string reason) : base(
        message: "Workflow is not sastified all requirement for the deactivation"
    )
    {
        Reason = reason;
    }
}
