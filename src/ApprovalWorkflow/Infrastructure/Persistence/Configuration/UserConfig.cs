using System;
using ApprovalWorkflow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApprovalWorkflow.Infrastructure.Persistence.Configuration;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure (EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Email)
        .HasMaxLength(255);
        builder.Property(t => t.Password)
        .HasMaxLength(255);
        builder.Property(t => t.DisplayName)
        .HasMaxLength(200);

        builder.HasIndex(t => t.Email);
        builder.HasIndex(t => t.DisplayName);
    }
}
