using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Request.GetRequestDetail;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.RequestTest;

public class RequestDetailGetTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public RequestDetailGetTest(WebAppFactory factory)
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

        var newUser2 = new User
        {
            Email = "example1@email.com",
            Password = "test123",
            DisplayName = "tester2"
        };
        dbContext.User.Add(newUser2);

        var newUser3 = new User
        {
            Email = "example2@email.com",
            Password = "test123",
            DisplayName = "tester2"
        };
        dbContext.User.Add(newUser3);

        var newWp = new WorkSpace
        {
            Name = "test workspace",
            CreatedBy = newUser.Id
        };
        dbContext.WorkSpace.Add(newWp);

        await dbContext.SaveChangesAsync();


        var newWpMember = new WorkSpaceMember
        {
            UserId = newUser.Id,
            WorkSpaceId = newWp.Id
        };
        var newWpMember2 = new WorkSpaceMember
        {
            UserId = newUser2.Id,
            WorkSpaceId = newWp.Id
        };
        var newWpMember3 = new WorkSpaceMember
        {
            UserId = newUser3.Id,
            WorkSpaceId = newWp.Id
        };
        dbContext.WorkSpaceMember.AddRange(newWpMember, newWpMember2, newWpMember3);

        var newWf = new WorkFlow
        {
            WorkSpaceId = newWp.Id,
            Name = "testing",
            Description = "test",
            CreatedBy = newUser.Id,
            Status = WorkflowStatus.Active
        };
        dbContext.WorkFlow.Add(newWf);

        var userAccessToken = jwtGenerator.GenerateToken(userId: newUser.Id);

        var user2AccessToken = jwtGenerator.GenerateToken(userId: newUser2.Id);

        var user3AccessToken = jwtGenerator.GenerateToken(userId: newUser3.Id);

        var newWfStep = new WorkFlowSteps
        {
            WorkFlowId = newWf.Id,
            Name = "Test wf",
            StepOrder = 1,
            ApprovedUserId = newUser2.Id
        };

        var newWfStep2 = new WorkFlowSteps
        {
            WorkFlowId = newWf.Id,
            Name = "Test wf",
            StepOrder = 2,
            ApprovedUserId = newUser3.Id
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
            ApproverUserId = newUser2.Id,
            Status = ApprovalStatus.Pending
        };

        var newRequestApr2 = new RequestApproval
        {
            RequestId = newRequest.Id,
            WorkFlowStepId = newWfStep2.Id,
            StepOrder = newWfStep2.StepOrder,
            StepName = newWfStep2.Name,
            ApproverUserId = newUser3.Id,
            Status = ApprovalStatus.Pending
        };

        dbContext.RequestApproval.AddRange(newRequestApr, newRequestApr2);

        await dbContext.SaveChangesAsync();


        //When
        // var response = await _client.GetAsync(
        //     requestUri: $"/api/v1/request/{newRequest.Id}/detail"
        // );


        //Then
        var tokens = new List<string>
        {
            userAccessToken,
            user2AccessToken,
            user3AccessToken
        };

        foreach(var token in tokens)
        {
            using var request = new HttpRequestMessage(
                method: HttpMethod.Get,
                requestUri: $"/api/v1/request/{newRequest.Id}/detail"
            );

            request.Headers.Add(
                name: "Cookie",
                value: $"access_token={token}"
            );

            using var response = await _client.SendAsync(request);

             Assert.NotNull(response);
        
            Assert.Equal(
                expected: HttpStatusCode.OK,
                actual: response.StatusCode
            );

            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<RequestDetailGetResult>>();

            await using var testScope = _factory.Services.CreateAsyncScope();
            var testDb = testScope.ServiceProvider.GetRequiredService<AppDbContext>();

            var validReq = await testDb.Requests.FirstOrDefaultAsync(
                t => t.Id == newRequest.Id
            );
            Assert.Equal(
                expected: validReq.Id,
                actual: responseData.Data.RequestId
            );
            Assert.Equal(
                expected: validReq.WorkFlowId,
                actual: responseData.Data.WorkflowId
            );
            Assert.Equal(
                expected: validReq.WorkSpaceId,
                actual: responseData.Data.WorkspaceId
            );
            Assert.Equal(
                expected: validReq.CreatedBy,
                actual: responseData.Data.CreatedBy
            );
        }
    }
}
