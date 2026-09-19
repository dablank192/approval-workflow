using System;
using Register = ApprovalWorkflow.Feature.Auth.Register;
using ApprovalWorkflow.Tests.Infrastructure;
using System.Net.Http.Json;
using System.Net;
using ApprovalWorkflow.Feature.Auth.Register;

namespace ApprovalWorkflow.Tests.Feature.AuthTest;

public class RegisterAuthTest : IClassFixture<WebAppFactory>
{
    public HttpClient _client;

    public RegisterAuthTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Execute()
    {

        //Given
        var payload = new Register.AuthRegisterCommand(
            DisplayName: "admin",
            Email: "example@email.com",
            Password: "abc123"
        );


        //When
        var request = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            payload
        );
        var response = await request.Content.ReadFromJsonAsync<Register.AuthRegisterResult>();


        //Then
        Assert.Equal(
            HttpStatusCode.Created,
            request.StatusCode
        );

        Assert.NotNull(response);
    }
}
