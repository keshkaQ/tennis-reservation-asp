using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(c => c.Id).HasName("pk_users");

            builder.Property(c => c.Id).HasColumnName("id").
                HasConversion(id => id.Value,value => new UserId(value))     
                .IsRequired();


            builder.Property(c => c.FirstName)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasColumnName("first_name");

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("last_name");

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");

            builder.Property(c => c.PhoneNumber)
              .IsRequired()
              .HasMaxLength(20)
              .HasColumnName("phone_number");

            builder.Property(c => c.RegistrationDate)
                .HasColumnName("registration_date")
                 .IsRequired();

            builder.HasIndex(c => c.Email)
               .IsUnique()
               .HasDatabaseName("ix_users_email");

            builder.HasIndex(c => c.PhoneNumber)
                .HasDatabaseName("ix_users_phone");

            builder.HasMany(c => c.Reservations)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
