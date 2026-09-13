using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprovalWorkflow.Infrastructure.Persistence.Configuration;

public class WorkFlowConfig : IEntityTypeConfiguration<WorkFlow>
{
    public void Configure(EntityTypeBuilder<WorkFlow> builder)
    {
        builder.ToTable("WorkFlow");
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.Name);

        builder.Property(t => t.Name)
        .HasMaxLength(250);
        builder.Property(t => t.Description)
        .HasColumnType("text");
        builder.Property(t => t.Status)
        .HasMaxLength(100);

        builder.HasOne(t => t.WorkSpace)
        .WithMany(t => t.WorkFlow)
        .HasForeignKey(t => t.WorkSpaceId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
        .WithMany(t => t.WorkFlow)
        .HasForeignKey(t => t.CreatedBy)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
