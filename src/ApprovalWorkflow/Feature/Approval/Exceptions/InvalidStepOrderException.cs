using System;

namespace ApprovalWorkflow.Feature.Approval.Exceptions;


public class InvalidStepOrderException : System.Exception
{
    public string Reason {get;}
    public InvalidStepOrderException(string reason) : base(
        message: "Invalid step-order"
    )
    {
        Reason = reason;
    }
}
