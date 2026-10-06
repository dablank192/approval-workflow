using System;

namespace ApprovalWorkflow.Feature.Request.Exceptions;

public class InvalidRequestStatusException : System.Exception
{
    public string Reason{get;}

    public InvalidRequestStatusException(string reason) : base(
        message: "Request's status is invalid"
    )
    {
        Reason = reason;
    }
}
