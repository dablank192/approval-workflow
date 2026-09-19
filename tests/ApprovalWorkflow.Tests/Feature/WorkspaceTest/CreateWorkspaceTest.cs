using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Feature.Workspace.CreateWorkspace;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using workflow = ApprovalWorkflow.Feature.Workspace.CreateWorkspace;

namespace ApprovalWorkflow.Tests.Feature.WorkspaceTest;

public class CreateWorkspaceTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public CreateWorkspaceTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        //Given
        var newUser = new User
        {
            Email = "example@email.com",
            Password = "abc1234",
            DisplayName = "Workspace Tester"
        };

        string accessToken;

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtgenerator>();
        
        dbContext.User.Add(newUser);
        await dbContext.SaveChangesAsync();

        accessToken = jwtGenerator.GenerateToken(userId: newUser.Id);

        _client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: $"access_token={accessToken}" 
        );

        var payload = new workflow.SubCommand(
            WorkspaceName: "Workspace Test"
        );


        //When
        var response = await _client.PostAsJsonAsync(
            requestUri: "/api/v1/workspace/new",
            value: payload
        );


        //Then
        Assert.Equal(
            expected: HttpStatusCode.Created,
            actual: response.StatusCode
        );

        await using var verifyScope = _factory.Services.CreateAsyncScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var workspace = await verifyDbContext.WorkSpace.SingleAsync(
            t => t.Name == "Workspace Test"
        );

        Assert.Equal(
            expected: newUser.Id,
            actual: workspace.CreatedBy
        );

        var membership = await verifyDbContext.WorkSpaceMember.SingleAsync(
            t => t.WorkSpaceId == workspace.Id
            &&
            t.UserId == newUser.Id
        );

        Assert.Equal(
            expected: MemberRoles.Owner,
            actual: membership.Roles
        );
    }
}
