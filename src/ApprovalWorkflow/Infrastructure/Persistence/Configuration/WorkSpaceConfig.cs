using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprovalWorkflow.Infrastructure.Persistence.Configuration;

public class WorkSpaceConfig : IEntityTypeConfiguration<WorkSpace>
{
    public void Configure(EntityTypeBuilder<WorkSpace> builder)
    {
        builder.ToTable("WorkSpace");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
        .HasMaxLength(200);

        builder.HasIndex(t => t.Name);

        builder.HasOne(t => t.User)
        .WithMany(t => t.WorkSpace)
        .HasForeignKey(t => t.CreatedBy)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
