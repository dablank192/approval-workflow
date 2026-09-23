using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Auth.Exceptions;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Feature.Auth.Login;

public record AuthLoginCommand(
    string Email,
    string Password
) : IRequest<AuthLoginResult>;
public record AuthLoginResult(
    Guid UserId
);

public class AuthLogin(
    ISender sender,
    IJwtgenerator jwtGenerator
) : AuthApi
{
    [HttpPost("login", Name = "AuthLogin")]
    [ProducesResponseType<ApiResponse<AuthLoginResult>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleAsync([FromBody]AuthLoginCommand req)
    {
        var result = await sender.Send(req);

        var accessToken = jwtGenerator.GenerateToken(result.UserId);

        Response.Cookies.Append(
            key: "access_token",
            value: accessToken,
            options: new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(30)
            }
        );

        var response = ApiResponse<AuthLoginResult>.Success(
            data: result,
            message: "success"
        );

        return Ok(response);
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<AuthLoginCommand, AuthLoginResult>
{
    public async Task<AuthLoginResult> Handle(AuthLoginCommand req, CancellationToken ct)
    {
        var validEmail = await dbContext.User.FirstOrDefaultAsync(
            t => t.Email == req.Email,
            ct
        )
        ?? throw new InvalidUsernameOrPasswordException("Incorrect Email");

        var validPassword = Hasher.Validate(
            currentPassword: req.Password,
            hashedPassword: validEmail.Password
        );

        if(validPassword == false) throw new InvalidUsernameOrPasswordException("Incorrect Password");

        return new AuthLoginResult(UserId: validEmail.Id);
    }
}