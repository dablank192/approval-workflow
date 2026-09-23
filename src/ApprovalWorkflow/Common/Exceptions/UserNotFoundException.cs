using System;
using Microsoft.AspNetCore.WebUtilities;

namespace ApprovalWorkflow.Common.Exceptions;


public class UserNotFoundException : System.Exception
{
    public string Reason {get;}
    public UserNotFoundException(string reason) : base(
        message: "User Not Found"
    )
    {
        Reason = reason;
    }
}
