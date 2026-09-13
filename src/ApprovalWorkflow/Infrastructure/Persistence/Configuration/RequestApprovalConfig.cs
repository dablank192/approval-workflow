using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprovalWorkflow.Infrastructure.Persistence.Configuration;

public class RequestApprovalConfig : IEntityTypeConfiguration<RequestApproval>
{
    public void Configure(EntityTypeBuilder<RequestApproval> builder)
    {
        builder.ToTable("RequestApproval");
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.StepName);
        builder.HasIndex(t => t.Status);

        builder.Property(t => t.StepOrder)
        .HasMaxLength(150);
        builder.Property(t => t.StepName)
        .HasMaxLength(250);
        builder.Property(t => t.Status)
        .HasMaxLength(150);
        builder.Property(t => t.Comment)
        .HasColumnType("text");

        builder.HasOne(t => t.Requests)
        .WithMany(t => t.RequestApproval)
        .HasForeignKey(t => t.RequestId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.WorkFlow)
        .WithMany(t => t.RequestApproval)
        .HasForeignKey(t => t.WorkFlowId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.User)
        .WithMany(t => t.RequestApproval)
        .HasForeignKey(t => t.ApprovalUserId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
