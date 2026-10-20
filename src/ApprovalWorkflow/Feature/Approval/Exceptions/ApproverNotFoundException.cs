using System;

namespace ApprovalWorkflow.Feature.Approval.Exceptions;

public class ApproverNotFoundException : System.Exception
{
    private string Reason {get;}

    public ApproverNotFoundException(string reason) : base(
        message: "Approver's Id not found"
    )
    {
        Reason = reason;
    }
}
