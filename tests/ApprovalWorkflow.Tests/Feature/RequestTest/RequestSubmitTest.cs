using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Request.SubmitRequest;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.RequestTest;

public class RequestSubmitTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public RequestSubmitTest(WebAppFactory factory)
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

        await dbContext.SaveChangesAsync();

        var accessToken = jwtGenerator.GenerateToken(userId: newUser.Id);

        var newWpMember = new WorkSpaceMember
        {
            UserId = newUser.Id,
            WorkSpaceId = newWp.Id
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
        dbContext.WorkFlow.Add(newWf);

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

        var newRequest = new Requests
        {
            WorkSpaceId = newWp.Id,
            WorkFlowId = newWf.Id,
            CreatedBy = newUser.Id,
            Title = "test",
            Description = "testing",
            Status = RequestStatus.Draft
        };
        dbContext.Requests.Add(newRequest);

        await dbContext.SaveChangesAsync();


        //When
        var response = await _client.PostAsync(
            requestUri: $"/api/v1/request/{newRequest.Id}/submit",
            content: null
        );

        
        //Then
        Assert.NotNull(response);

        Assert.Equal(
            expected: HttpStatusCode.Created,
            actual: response.StatusCode
        );

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<RequestSubmitResult>>();
        await using var testScope = _factory.Services.CreateAsyncScope();
        var testDbContext = testScope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var validRequest = await testDbContext.Requests.FirstOrDefaultAsync(
            t => t.WorkFlowId == newWf.Id
        );

        var validWf = await testDbContext.WorkFlow
        .Include(t => t.WorkFlowSteps)
        .FirstOrDefaultAsync(
            t => t.Id == newWf.Id
        );

        var totalWfStep = validWf.WorkFlowSteps.Count;

        Assert.Equal(
            expected: totalWfStep,
            actual: responseData.Data.ApprovalStepCount
        );

        Assert.Equal(
            expected: validRequest.Id,
            actual: responseData.Data.RequestId
        );

        Assert.Equal(
            expected: RequestStatus.Pending,
            actual: responseData.Data.RequestStatus
        );
    }
}
