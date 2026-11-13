using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Workflow.GetWorkflow;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet.Sftp;

namespace ApprovalWorkflow.Tests.Feature.WorkflowTest;

public class WorkflowGetTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public WorkflowGetTest(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        //given
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

        await dbContext.SaveChangesAsync();

        var accessToken = jwtGenerator.GenerateToken(userId: newUser.Id);

        var newWpMember = new WorkSpaceMember
        {
            UserId = newUser.Id,
            WorkSpaceId = newWp.Id,
            Status = MemberStatus.Active
        };
        dbContext.WorkSpaceMember.Add(newWpMember);

        var newWf = new WorkFlow
        {
            WorkSpaceId = newWp.Id,
            Name = "testing",
            Description = "test",
            CreatedBy = newUser.Id,
            Status = WorkflowStatus.Active
        };

        var newWf2 = new WorkFlow
        {
            WorkSpaceId = newWp.Id,
            Name = "Testing2",
            Description = "",
            CreatedBy = newUser.Id,
            Status = WorkflowStatus.Draft
        };

        dbContext.WorkFlow.AddRange(newWf, newWf2);

        _client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: $"access_token={accessToken}"
        );

        var newWfStep = new WorkFlowSteps
        {
            WorkFlowId = newWf.Id,
            Name = "Test wf",
            StepOrder = 1,
            ApprovedUserId = newUser.Id
        };

        var newWfStep2 = new WorkFlowSteps
        {
            WorkFlowId = newWf.Id,
            Name = "Test wf",
            StepOrder = 2,
            ApprovedUserId = newUser.Id
        };
        dbContext.WorkFlowSteps.AddRange(newWfStep, newWfStep2);

        await dbContext.SaveChangesAsync();

        //When
        var response = await _client.GetAsync(
            requestUri: $"/api/v1/workflow/{newWp.Id}"
        );


        //Then
        Assert.NotNull(response);
        Assert.Equal(
            expected: HttpStatusCode.OK,
            actual: response.StatusCode
        );

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<List<WorkflowGetResult>>>();

        var firstRecord = responseData.Data[0];

        await using var testScope = _factory.Services.CreateAsyncScope();
        var testDbContext = testScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var validwf = testDbContext.WorkFlow.FirstOrDefault(
            t => t.WorkSpaceId == newWp.Id
            &&
            t.Id == newWf2.Id
        );

        Assert.Equal(
            expected: validwf.WorkSpaceId,
            actual: firstRecord.WorkspaceId
        );

        Assert.Equal(
            expected: validwf.Id,
            actual: firstRecord.WorkflowId
        );

        Assert.Equal(
            expected: validwf.Name,
            actual: firstRecord.WorkflowName
        );

        Assert.Equal(
            expected: validwf.Status,
            actual: firstRecord.Status
        );
    }
}
