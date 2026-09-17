using System;

namespace ApprovalWorkflow.Feature.Auth.Exceptions;

public class InvalidUsernameOrPasswordException : SystemException
{
    public string Reason {get;}
    public InvalidUsernameOrPasswordException(string reason) : base(
        message: "Invalid Credential"
    )
    {
        Reason = reason;
    }
}
