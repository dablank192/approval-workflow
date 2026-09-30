using System;

namespace ApprovalWorkflow.Feature.Workflow.Exceptions;

public class InvalidWorkflowCreationException : System.Exception
{
    public string Reason{get;}
    
    public InvalidWorkflowCreationException(string reason) : base(
        message: "Invalid Workflow Creation"
    )
    {
        Reason = reason;
    }
}
