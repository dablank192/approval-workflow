using System;

namespace ApprovalWorkflow.Feature.Workflow.Exceptions;

public class WorkflowNotFoundException : System.Exception
{
    public string Reason {get;}

    public WorkflowNotFoundException(string reason) : base(
        message: "Workflow not found or not existed"
    )
    {
        Reason = reason;
    }
}
