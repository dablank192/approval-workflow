using System;

namespace ApprovalWorkflow.Feature.Workflow.Exceptions;

public class InvalidWorkflowActivationException : System.Exception
{
    public string Reason {get;}

    public InvalidWorkflowActivationException(string reason) : base(
        message: "Workflow is not sastified all requirement for the activation"
    )
    {
        Reason = reason;
    }
}
