using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprovalWorkflow.Infrastructure.Persistence.Configuration;

public class RequestsConfig : IEntityTypeConfiguration<Requests>
{
    public void Configure(EntityTypeBuilder<Requests> builder)
    {
        builder.ToTable("Requests");
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.Title);

        builder.Property(t => t.Title)
        .HasMaxLength(250);
        builder.Property(t => t.Description)
        .HasColumnType("text");
        builder.Property(t => t.Status)
        .HasMaxLength(100);

        builder.HasOne(t => t.WorkFlow)
        .WithMany(t => t.Requests)
        .HasForeignKey(t => t.WorkFlowId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.WorkSpace)
        .WithMany(t => t.Requests)
        .HasForeignKey(t => t.WorkSpaceId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.User)
        .WithMany(t => t.Requests)
        .HasForeignKey(t => t.CreatedBy)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
