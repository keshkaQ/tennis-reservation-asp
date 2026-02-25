using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres
{
    public class TennisReservationDbContext: DbContext, IReadDbContext
    {
        public DbSet<TennisCourt> TennisCourts => Set<TennisCourt>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<UserCredentials> UserCredentials => Set<UserCredentials>();

        public IQueryable<User> UsersRead => Set<User>().AsNoTracking();
        public IQueryable<UserCredentials> UserCredentialsRead => Set<UserCredentials>().AsNoTracking();
        public IQueryable<Reservation> ReservationsRead => Set<Reservation>().AsNoTracking();
        public IQueryable<TennisCourt> TennisCourtsRead => Set<TennisCourt>().AsNoTracking();

        public TennisReservationDbContext(DbContextOptions<TennisReservationDbContext> options)
            : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TennisReservationDbContext).Assembly);
        }

        public static ILoggerFactory CreateLoggerFactory() => LoggerFactory.Create(builder => { builder.AddConsole(); });
    }
}
