using System;

namespace ApprovalWorkflow.Feature.Request.Exceptions;

public class InvalidRequestAccessException : System.Exception
{
    public string Reason {get;}

    public InvalidRequestAccessException(string reason) : base(
        message: "Invalid Member status or Request's location"
    )
    {
        Reason = reason;
    }
}
