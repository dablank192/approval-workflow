using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Workspace.GetAllMember;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApprovalWorkflow.Tests.Feature.WorkspaceTest;

public class GetAllMemberTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public GetAllMemberTest(WebAppFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task ExecuteAsync()
    {
        //Given
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtgenerator>();

        var newUser = new User
        {
            Email = "example123@email.com",
            Password = "tester123",
            DisplayName = "Tester"
        };
        dbContext.User.Add(newUser);

        var newWorkspace = new WorkSpace
        {
            Name = "Test123",
            CreatedBy = newUser.Id
        };
        dbContext.WorkSpace.Add(newWorkspace);

        var newWorkspaceMember = new WorkSpaceMember
        {
            UserId = newUser.Id,
            WorkSpaceId = newWorkspace.Id
        };
        dbContext.WorkSpaceMember.Add(newWorkspaceMember);

        await dbContext.SaveChangesAsync();

        var payload = new GetAllMemberSubQuery(
            WorkspaceId: newWorkspace.Id,
            Roles: MemberRoles.Member
        );

        var accessToken = jwtService.GenerateToken(userId: newUser.Id);
        _client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: $"access_token={accessToken}"
        );


        //When
        var response = await _client.GetAsync(
            requestUri: $"/api/v1/workspace/members?workspaceid={payload.WorkspaceId}&roles={payload.Roles}"            
        );


        //Then
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.NotNull(response);

        Assert.Equal(
            expected: HttpStatusCode.OK,
            actual: response.StatusCode
        );

        var httpResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<GetAllMemberResult>>>();

        var members = Assert.Single(httpResponse.Data);

        var verifyUser = await verifyDb.User.SingleAsync(t => t.Id == newUser.Id);
        
        Assert.Equal(
            expected: verifyUser.Id,
            actual: members.UserId
        );

        var verifyRoles = await verifyDb.WorkSpaceMember.SingleAsync(t => t.UserId == newUser.Id);

        Assert.Equal(
            expected: verifyRoles.Roles,
            actual: members.Roles
        );
    }
}
