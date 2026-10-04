using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Workflow.AddWorkflowStep;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.WorkflowTest;

public class WorkflowStepAddTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public WorkflowStepAddTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        //Given
        await using var scope = _factory.Services.CreateAsyncScope();
        var testDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var testJwt = scope.ServiceProvider.GetRequiredService<IJwtgenerator>();


        var newUser = new User
        {
            Email = "test123@example.com",
            Password = "tester123",
            DisplayName = "tester3"
        };
        testDb.User.Add(newUser);

        var newWp = new WorkSpace
        {
            Name = "test wp",
            CreatedBy = newUser.Id
        };
        testDb.WorkSpace.Add(newWp);

        var newWpMember = new WorkSpaceMember
        {
            UserId = newUser.Id,
            WorkSpaceId = newWp.Id
        };
        testDb.WorkSpaceMember.Add(newWpMember);

        var newWf = new WorkFlow
        {
            WorkSpaceId = newWp.Id,
            Name = "testing",
            Description = "test",
            CreatedBy = newUser.Id
        };
        testDb.WorkFlow.Add(newWf);
        
        await testDb.SaveChangesAsync();

        var accessToken = testJwt.GenerateToken(userId: newUser.Id);
        _client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: $"access_token={accessToken}"
        );

        var payload = new WorkflowStepAddSubCommand(
            WorkflowId: newWf.Id,
            ApprovedUserId: newUser.Id,
            Name: "test step"
        );


        //When
        var response = await _client.PostAsJsonAsync(
            requestUri: "/api/v1/workflow/step/new",
            value: payload
        );


        //Then
        await using var testValidScope = _factory.Services.CreateAsyncScope();
        var testValidDb = testValidScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var testValidWf = await testValidDb.WorkFlowSteps.FirstOrDefaultAsync(
            t => t.WorkFlowId == newWf.Id
        );

        
        Assert.NotNull(response);

        Assert.Equal(
            expected: HttpStatusCode.Created,
            actual: response.StatusCode
        );

        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse<WorkflowStepAddResult>>();

        Assert.Equal(
            expected: testValidWf!.WorkFlowId,
            actual: responseBody!.Data!.WorkflowId
        );

        Assert.Equal(
            expected: 1,
            actual: responseBody!.Data.WorkflowStep
        );
    }
}
