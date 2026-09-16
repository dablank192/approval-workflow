using System;
using Microsoft.AspNetCore.Identity;

namespace ApprovalWorkflow.Infrastructure.Authentication;

public static class Hasher
{
    public static string Hash(string currentPassword)
    {
        var hashedPassword = new PasswordHasher<object>()
        .HashPassword(new object(), password: currentPassword);

        return hashedPassword;
    }

    public static bool Validate(string currentPassword, string hashedPassword)
    {
        var validationResult = new PasswordHasher<object>()
        .VerifyHashedPassword(
            new object(),
            hashedPassword: hashedPassword,
            providedPassword: currentPassword
        );

        if(validationResult == PasswordVerificationResult.Success) return true;
        else if (validationResult == PasswordVerificationResult.Failed) return false;
        else throw new Exception();
    }
}
