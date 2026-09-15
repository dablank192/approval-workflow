using System;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Auth.Register;

public record Command(
    string DisplayName,
    string Email,
    string Password
) : IRequest<Result>;
public record Result(
    Guid UserId
);

public class UserRegister(
    ISender sender
) : AuthApi
{
    [HttpPost("register", Name = "AuthRegister")]
    [ProducesResponseType<Result>(StatusCodes.Status201Created)]
    public async Task<IActionResult> HandleAsync([FromBody] Command req, CancellationToken ct)
    {
        var result = await sender.Send(req, ct);

        return CreatedAtRoute(
            routeName: "AuthRegister",
            routeValues: null,
            value: result
        );
    }
}

public class Handler(
    AppDbContext dbContext
) : IRequestHandler<Command, Result>
{
    public async Task<Result> Handle(Command req, CancellationToken ct)
    {
        var newUser = new User
        {
            Email = req.Email,
            Password = req.Password,
            DisplayName = req.DisplayName
        };

        dbContext.User.Add(newUser);
        await dbContext.SaveChangesAsync(ct);

        return new Result(
            UserId: newUser.Id
            );
    }
}
