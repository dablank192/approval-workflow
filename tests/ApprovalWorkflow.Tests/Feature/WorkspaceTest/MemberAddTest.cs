using System;
using System.Net;
using System.Net.Http.Json;
using ApprovalWorkflow.Common.Api;
using ApprovalWorkflow.Feature.Workspace.AddMembers;
using ApprovalWorkflow.Infrastructure.Authentication;
using ApprovalWorkflow.Infrastructure.Persistence;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using ApprovalWorkflow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Ocsp;
using Renci.SshNet.Sftp;

namespace ApprovalWorkflow.Tests.Feature.WorkspaceTest;

public class MemberAddTest : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;

    public MemberAddTest(WebAppFactory factory)
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

        var payload = new MemberAddCommand(
            UserId: newUser.Id,
            WorkspaceId: newWorkspace.Id,
            Roles: MemberRoles.Member
        );
        
        await dbContext.SaveChangesAsync();

        var accessToken = jwtService.GenerateToken(userId: newUser.Id);
        _client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: $"access_token={accessToken}"
        );


        //When
        var response = await _client.PostAsJsonAsync(
            requestUri: "/api/v1/workspace/new-member",
            value: payload
        );

        
        //Then
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var verifiedUser = await verifyDb.User.SingleAsync(t => t.Id == newUser.Id);
        var verifiedMember = await verifyDb.WorkSpaceMember.SingleAsync(t => t.UserId == newUser.Id);
        var verifiedWp = await verifyDb.WorkSpace.SingleAsync(t => t.Id == newWorkspace.Id);

        Assert.NotNull(response);

        Assert.Equal(
            expected: verifiedUser.Id,
            actual: verifiedMember.UserId
        );

        Assert.Equal(
            expected: verifiedWp.Id,
            actual: verifiedMember.WorkSpaceId
        );

        Assert.Equal(
            expected: HttpStatusCode.Created,
            actual: response.StatusCode
        );
    }
}
