using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Approval.ApproveRequest;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.Approval;

public class ApproveRequestTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public ApproveRequestTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task ExecuteAsync()
    {
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
            Status = RequestStatus.Pending
        };
        dbContext.Requests.Add(newRequest);

        var newRequestApr = new RequestApproval
        {
            RequestId = newRequest.Id,
            WorkFlowStepId = newWfStep.Id,
            StepOrder = newWfStep.StepOrder,
            StepName = newWfStep.Name,
            ApproverUserId = newUser.Id,
            Status = ApprovalStatus.Pending
        };
        dbContext.RequestApproval.Add(newRequestApr);

        await dbContext.SaveChangesAsync();

        
        //When
        var response = await _client.PostAsync(
            requestUri: $"/api/v1/approval/{newRequestApr.Id}/approve",
            content: null
        );


        //Then
        Assert.NotNull(response);

        Assert.Equal(
            expected: HttpStatusCode.OK,
            actual: response.StatusCode
        );

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<ApproveRequestResult>>();

        await using var testScope = _factory.Services.CreateAsyncScope();
        var testDb = testScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var testReqApr = await testDb.RequestApproval
        .FirstOrDefaultAsync(
            t => t.Id == newRequestApr.Id
        );

        Assert.Equal(
            expected: testReqApr.Id,
            actual: responseData.Data.RequestApprovalId
        );

        Assert.Equal(
            expected: testReqApr.RequestId,
            actual: responseData.Data.RequestId
        );

        Assert.Equal(
            expected: ApprovalStatus.Approved,
            actual: testReqApr.Status
        );
    }
}
