using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Configurations
{
    public class TennisCourtConfiguration : IEntityTypeConfiguration<TennisCourt>
    {
        public void Configure(EntityTypeBuilder<TennisCourt> builder)
        {
            builder.ToTable("courts");

            builder.HasKey(c => c.Id).HasName("pk_courts");

            builder.Property(c => c.Id).HasColumnName("id")
                .HasConversion(
                id => id.Value,
                value => new TennisCourtId(value)
                ).IsRequired();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");

            builder.Property(c => c.HourlyRate)
                .IsRequired()
                .HasColumnName("hourly_rate")
                .HasColumnType("decimal(10,2)");


            builder.Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("description");

            builder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("ix_courts_name");

            builder.HasIndex(c => c.HourlyRate)
                .HasDatabaseName("ix_courts_hourly_rate");

            builder.HasMany(c => c.Reservations)
                .WithOne(r => r.TennisCourt)
                .HasForeignKey(r => r.TennisCourtId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
