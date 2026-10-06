using System;

namespace ApprovalWorkflow.Common.Exceptions;

public class RequestNotFoundException : System.Exception
{
    public string Reason {get;}
    public RequestNotFoundException(string reason) : base (
        message: "Request not found"
    )
    {
        Reason = reason;
    }
}
