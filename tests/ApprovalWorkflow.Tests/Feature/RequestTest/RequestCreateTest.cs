using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Request.CreateRequest;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.RequestTest;

public class RequestCreateTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public RequestCreateTest(WebAppFactory factory)
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
            CreatedBy = newUser.Id
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
        dbContext.WorkFlowSteps.Add(newWfStep);

        await dbContext.SaveChangesAsync();

        var payload = new RequestCreateCommand(
            WorkspaceId: newWp.Id,
            WorkflowId: newWf.Id,
            CreatedBy: newUser.Id,
            Title: "Test request",
            Description: "testing"
        );


        //when
        var response = await _client.PostAsJsonAsync(
            requestUri: "/api/v1/request",
            value: payload
        );


        //Then
        Assert.NotNull(response);

        Assert.Equal(
            expected: HttpStatusCode.Created,
            actual: response.StatusCode
        );

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<RequestCreateResult>>();
        await using var testScope = _factory.Services.CreateAsyncScope();
        var testDbContext = testScope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var validRequest = await testDbContext.Requests.FirstOrDefaultAsync(
            t => t.WorkFlowId == newWf.Id
        );

        Assert.Equal(
            expected: "Request created successfully",
            actual: responseData.Data.Message
        );

        Assert.Equal(
            expected: validRequest.Id,
            actual: responseData.Data.RequestId 
        );
    }
}
