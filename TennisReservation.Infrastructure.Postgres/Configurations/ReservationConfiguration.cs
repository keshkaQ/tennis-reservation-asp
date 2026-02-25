using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("reservations");

            builder.HasKey(r => r.Id).HasName("pk_reservations");

            builder.Property(r => r.Id)
                .HasColumnName("id")
                .HasConversion(
                    id => id.Value,
                    value => new ReservationId(value))
                .IsRequired();

            builder.Property(r => r.TennisCourtId)
                .IsRequired()
                .HasConversion(
                id => id.Value,
                value => new TennisCourtId(value))
                .HasColumnName("court_id");

            builder.Property(r => r.UserId)
                .IsRequired()
                .HasConversion(
                id => id.Value,
                value => new UserId(value))
                .HasColumnName("user_id");

            builder.Property(r => r.TotalCost)
                .IsRequired()
                .HasColumnName("total_cost")
                .HasColumnType("decimal(10,2)");

            builder.Property(r => r.StartTime)
                .IsRequired()
                .HasColumnName("start_time");

            builder.Property(r => r.EndTime)
                .IsRequired()
                .HasColumnName("end_time");

            builder.Property(r => r.CreatedAt)
               .HasColumnName("created_at");

            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasColumnName("status");

            builder.HasIndex(c => c.Status)
                .HasDatabaseName("ix_reservations_status");

            builder.HasIndex(c => c.TotalCost)
                .HasDatabaseName("ix_reservations_total_cost");

            builder.HasIndex(r => new { r.StartTime, r.EndTime })
               .HasDatabaseName("ix_reservations_dates");

            builder.HasOne(r => r.User)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.TennisCourt)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.TennisCourtId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
