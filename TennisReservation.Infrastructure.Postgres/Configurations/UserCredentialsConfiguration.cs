using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TennisReservation.Domain.Models;

public class UserCredentialsConfiguration : IEntityTypeConfiguration<UserCredentials>
{
    public void Configure(EntityTypeBuilder<UserCredentials> builder)
    {
        builder.ToTable("user_credentials");

        builder.HasKey(c => c.Id).HasName("pk_user_credentials");

     
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new CredentialsId(value));

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        builder.Property(c => c.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.LastLoginAt)
            .HasColumnName("last_login_at")
            .IsRequired(false);  // Может быть null

        builder.Property(c => c.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0)  // По умолчанию 0
            .IsRequired();

        builder.Property(c => c.LockedUntil)
            .HasColumnName("locked_until")
            .IsRequired(false);  // Может быть null

        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("ix_user_credentials_user_id");

        builder.HasIndex(c => c.LockedUntil)
            .HasDatabaseName("ix_user_credentials_locked_until");

        builder.HasOne(c => c.User)
            .WithOne(u => u.Credentials)
            .HasForeignKey<UserCredentials>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}