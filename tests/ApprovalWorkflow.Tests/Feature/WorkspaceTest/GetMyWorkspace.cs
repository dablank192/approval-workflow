using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Workspace.GetMyWorkspace;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.WorkspaceTest;

public class GetMyWorkspaceTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public GetMyWorkspaceTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        //Given
        string accessToken;

        var newUser = new User
        {
            Email = "tester@email.com",
            Password = "test123",
            DisplayName = "helloworld123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        using var scope = _factory.Services.CreateScope();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtgenerator>();

        dbContext.User.Add(newUser);

        var newWp = new WorkSpace
        {
            Name = "test wp",
            CreatedBy = newUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.WorkSpace.Add(newWp);

        var newWpMember = new WorkSpaceMember
        {
            UserId = newUser.Id,
            WorkSpaceId = newWp.Id
        };
        dbContext.WorkSpaceMember.Add(newWpMember);

        await dbContext.SaveChangesAsync();

        accessToken = jwtGenerator.GenerateToken(userId: newUser.Id);
        _client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: $"access_token={accessToken}"
        );


        //When
        var response = await _client.GetAsync(
            requestUri: "/api/v1/workspace"
        );


        //Then
        Assert.Equal(
            expected: HttpStatusCode.OK,
            actual: response.StatusCode
        );

        using var validateDb = _factory.Services.GetRequiredService<AppDbContext>();
        var currentWp = await validateDb.WorkSpace.FindAsync(newWp.Id);

        var httpResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<MyWorkSpaceGetResult>>>();

        var workspace = Assert.Single(httpResponse.Data);

        Assert.Equal(
            expected: currentWp!.Id,
            actual: workspace.WorkspaceId
        );

        Assert.NotNull(httpResponse);

        Assert.Equal(
            expected: currentWp.Name,
            actual: workspace.Name
        );
    }
}
