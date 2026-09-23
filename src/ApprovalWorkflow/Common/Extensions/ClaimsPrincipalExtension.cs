using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApprovalWorkflow.Common.Extensions;

public static class ClaimsPrincipalExtension
{
    public static Guid GetCurrentUserId (this ClaimsPrincipal user)
    {
        var currentUser = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var userId = Guid.TryParse(currentUser, out Guid currentUserId);
        if(userId == false) throw new UnauthorizedAccessException("Can't find User Id");

        return currentUserId;
    }
}
