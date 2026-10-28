using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprovalWorkflow.Infrastructure.Persistence.Configuration;

public class WorkSpaceMemberConfig : IEntityTypeConfiguration<WorkSpaceMember>
{
    public void Configure(EntityTypeBuilder<WorkSpaceMember> builder)
    {
        builder.ToTable("WorkSpaceMember");
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => new
        {
            t.WorkSpaceId,
            t.UserId
        })
        .IsUnique();

        builder.Property(t => t.Roles)
        .HasMaxLength(100);
        builder.Property(t => t.Status)
        .HasMaxLength(100);

        builder.HasOne(t => t.WorkSpace)
        .WithMany(t => t.WorkSpaceMember)
        .HasForeignKey(t => t.WorkSpaceId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
        .WithMany(t => t.WorkspaceMember)
        .HasForeignKey(t => t.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
