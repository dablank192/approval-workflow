using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprovalWorkflow.Infrastructure.Persistence.Configuration;

public class WorkFlowStepsConfig : IEntityTypeConfiguration<WorkFlowSteps>
{
    public void Configure (EntityTypeBuilder<WorkFlowSteps> builder)
    {
        builder.ToTable("WorkFlowSteps");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
        .HasMaxLength(250);
        builder.Property(t => t.StepOrder)
        .HasMaxLength(100);

        builder.HasOne(t => t.WorkFlow)
        .WithMany(t => t.WorkFlowSteps)
        .HasForeignKey(t => t.WorkFlowId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
        .WithMany(t => t.WorkFlowSteps)
        .HasForeignKey(t => t.ApprovedUserId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
