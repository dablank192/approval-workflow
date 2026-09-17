using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ApprovalWorkflow.Infrastructure.Authentication;

public interface IJwtgenerator
{
    string GenerateToken(Guid userId);
}

public class JwtGenerator(
    IConfiguration _config
) : IJwtgenerator
{
    public string GenerateToken(Guid userId)
    {
        var jwtKey = _config["Jwt:Secret"]!;
        
        var claim = new Claim[]{
            new(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var getKey = new SymmetricSecurityKey(
            key: Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            key:getKey,
            algorithm: SecurityAlgorithms.HmacSha256 
        );

        var jwtToken = new JwtSecurityToken(
            claims: claim,
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddMinutes(30)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}
