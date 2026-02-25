using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Database
{
    public interface IReadDbContext
    {
        IQueryable<User> UsersRead { get; }
        IQueryable<UserCredentials> UserCredentialsRead { get; }
        IQueryable<Reservation> ReservationsRead { get; }
        IQueryable<TennisCourt> TennisCourtsRead { get; }
    }
}
