using System;

namespace ApprovalWorkflow.Feature.Request.Exceptions;

public class InvalidOwnerOrApproverException : System.Exception
{
    public string Reason{get;}

    public InvalidOwnerOrApproverException(string reason) : base(
        message: "Invalid Request's Owner or Approver Id"
    )
    {
        Reason = reason;
    }
}
