using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Approval.GetApprovalInbox;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.Approval;

public class ApprovalInboxGetTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public ApprovalInboxGetTest(WebAppFactory factory)
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
        var response = await _client.GetAsync(
            requestUri: "api/v1/approval/inbox"
        );


        //Then
        Assert.NotNull(response);

        Assert.Equal(
            expected: HttpStatusCode.OK,
            actual: response.StatusCode
        );

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<List<ApprovalInboxGetResult>>>();

        await using var testScope = _factory.Services.CreateAsyncScope();
        var testDbContext = testScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var validRequestApr = await testDbContext.RequestApproval
        .FirstOrDefaultAsync(
            t => t.RequestId == newRequest.Id
        );


        Assert.Equal(
            expected: validRequestApr.Id,
            actual: responseData.Data[0].RequestApprovalId
        );

        Assert.Equal(
            expected: validRequestApr.RequestId,
            actual: responseData.Data[0].RequestId
        );

        Assert.Equal(
            expected: validRequestApr.StepOrder,
            actual: responseData.Data[0].StepOrder
        );
    }
}
