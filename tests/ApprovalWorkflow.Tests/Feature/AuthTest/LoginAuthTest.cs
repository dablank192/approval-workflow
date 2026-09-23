using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Login = ApprovalWorkflow.Feature.Auth.Login;

namespace ApprovalWorkflow.Tests.Feature.AuthTest;

public class LoginAuthTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public LoginAuthTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        //Given
        string plainPassword = "abc1234";
        string hashedPassword = Hasher.Hash(plainPassword);
        
        var testData = new User
        {
            Email = "abc@email.com",
            Password =  hashedPassword,
            DisplayName = "Tester123"
        };

        await using var scope = _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.User.Add(testData);
        await dbContext.SaveChangesAsync();

        var payload = new Login.AuthLoginCommand(
            Email: "abc@email.com",
            Password: "abc1234"
        );


        //When
        var request = await _client.PostAsJsonAsync(
            requestUri: "/api/v1/auth/login",
            value: payload
        );


        //Then
        Assert.Equal(
            expected: HttpStatusCode.OK,
            actual: request.StatusCode
        );

        var responseCookie = request.Headers.GetValues("Set-Cookie");

        Assert.NotNull(responseCookie);
        Assert.NotEmpty(collection: responseCookie);
    }
}
