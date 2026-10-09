using System;

namespace ApprovalWorkflow.Feature.Approval.Exceptions;

public class InvalidReqApprovalStatusException : System.Exception
{
    private string Reason{get;}

    public InvalidReqApprovalStatusException(string reason) : base(
        message: "Invalid Request Approval Status"
    )
    {
        Reason = reason;
    }
}
