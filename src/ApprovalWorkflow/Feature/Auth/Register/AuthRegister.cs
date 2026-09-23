using System;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Auth.Register;

public record AuthRegisterCommand(
    string DisplayName,
    string Email,
    string Password
) : IRequest<AuthRegisterResult>;
public record AuthRegisterResult(
    Guid UserId
);

public class UserRegister(
    ISender sender
) : AuthApi
{
    [HttpPost("register", Name = "AuthRegister")]
    [ProducesResponseType<ApiResponse<AuthRegisterResult>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> HandleAsync([FromBody] AuthRegisterCommand req, CancellationToken ct)
    {
        var result = await sender.Send(req, ct);

        var response = ApiResponse<AuthRegisterResult>.Success(
            data: result,
            message: "Account created successfully"
        );

        return StatusCode(
            statusCode: StatusCodes.Status201Created,
            value: response
        );
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<AuthRegisterCommand, AuthRegisterResult>
{
    public async Task<AuthRegisterResult> Handle(AuthRegisterCommand req, CancellationToken ct)
    {
        var hashedPassword = Hasher.Hash(currentPassword: req.Password);
        
        var newUser = new User
        {
            Email = req.Email,
            Password = hashedPassword,
            DisplayName = req.DisplayName
        };

        dbContext.User.Add(newUser);
        await dbContext.SaveChangesAsync(ct);

        return new AuthRegisterResult(
            UserId: newUser.Id
            );
    }
}
