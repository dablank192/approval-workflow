using System;

namespace ApprovalWorkflow.Feature.Approval.Exceptions;

public class InvalidApproverException : System.Exception
{
    public string Reason {get;}
    public InvalidApproverException(string reason) : base(
        message: "Invalid Approver's Id"
    )
    {
        Reason = reason;
    }
}
