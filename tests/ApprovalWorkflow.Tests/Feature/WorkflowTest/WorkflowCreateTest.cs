using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Workflow.CreateWorkflow;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.WorkflowTest;

public class WorkflowCreateTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public WorkflowCreateTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        //Given
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtGenerator = scope.ServiceProvider.GetRequiredService<IJwtgenerator>();

        var newUser = new User
        {
            Email = "example@email.com",
            Password = "test123",
            DisplayName = "tester1"
        };
        dbContext.User.Add(newUser);

        var newWp = new WorkSpace
        {
            Name = "test workspace",
            CreatedBy = newUser.Id
        };
        dbContext.WorkSpace.Add(newWp);

        var newWpMember = new WorkSpaceMember
        {
            UserId = newUser.Id,
            WorkSpaceId = newWp.Id
        };

        dbContext.WorkSpaceMember.Add(newWpMember);

        await dbContext.SaveChangesAsync();

        var accessToken = jwtGenerator.GenerateToken(userId: newUser.Id);

        _client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: $"access_token={accessToken}"
        );

        var payload = new WorkflowCreateSubCommand(
            WorkspaceId: newWp.Id,
            Name: "Test workflow",
            Description: "Testing"
        );


        //When
        var response = await _client.PostAsJsonAsync(
            requestUri: "/api/v1/workflow/new",
            value: payload
        );


        //Then
        Assert.NotNull(response);
        Assert.Equal(
            expected: HttpStatusCode.Created,
            actual: response.StatusCode
        );

        var validResponse = response.Content.ReadFromJsonAsync<ApiResponse<WorkflowCreateResult>>();

        using var validDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var validWf = await validDbContext.WorkFlow.FirstOrDefaultAsync(
            t => t.Id == validResponse.Result.Data.WorkflowId
            &&
            t.WorkSpaceId == validResponse.Result.Data.WorkspaceId
        );

        Assert.Equal(
            expected: validWf.WorkSpaceId,
            actual: validResponse.Result.Data.WorkspaceId
        );
        Assert.Equal(
            expected: validWf.Id,
            actual: validResponse.Result.Data.WorkflowId
        );
    }
}
