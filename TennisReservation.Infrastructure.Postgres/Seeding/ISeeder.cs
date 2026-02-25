namespace TennisReservation.Infrastructure.Postgres.Seeding
{
    public interface ISeeder
    {
        Task SeedAsync();
    }
}
