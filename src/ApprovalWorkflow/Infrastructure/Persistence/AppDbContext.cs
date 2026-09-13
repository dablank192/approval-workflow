using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApprovalWorkflow.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> option) : base(option){}

    public DbSet<User> User {get; set;}
    public DbSet<WorkSpace> WorkSpace {get; set;}
    public DbSet<WorkSpaceMember> WorkSpaceMember {get; set;}
    public DbSet<WorkFlow> WorkFlow {get; set;}
    public DbSet<WorkFlowSteps> WorkFlowSteps {get; set;}
    public DbSet<Requests> Requests {get; set;}
    public DbSet<RequestApproval> RequestApproval {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
