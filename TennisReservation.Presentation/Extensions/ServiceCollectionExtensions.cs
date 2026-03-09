using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Application.Interfaces;
using TennisReservation.Application.Reservations.Commands.CancelReservation;
using TennisReservation.Application.Reservations.Commands.CreateReservation;
using TennisReservation.Application.Reservations.Commands.DeleteReservation;
using TennisReservation.Application.Reservations.Commands.UpdateReservation;
using TennisReservation.Application.Reservations.Interfaces;
using TennisReservation.Application.Reservations.Queries.GetAllReservationByStatus;
using TennisReservation.Application.Reservations.Queries.GetAllReservationsByDate;
using TennisReservation.Application.TennisCourts.Commands.CreateTennisCourt;
using TennisReservation.Application.TennisCourts.Commands.DeleteTennisCourt;
using TennisReservation.Application.TennisCourts.Commands.UpdateTennisCourt;
using TennisReservation.Application.TennisCourts.Interfaces;
using TennisReservation.Application.TennisCourts.Queries.GetAllReservationsByCourtId;
using TennisReservation.Application.TennisCourts.Queries.GetCourtAvailability;
using TennisReservation.Application.Users.Auth;
using TennisReservation.Application.Users.Commands.ChangePassword;
using TennisReservation.Application.Users.Commands.ChangeRole;
using TennisReservation.Application.Users.Commands.CreateUser;
using TennisReservation.Application.Users.Commands.DeleteUser;
using TennisReservation.Application.Users.Commands.LockUser;
using TennisReservation.Application.Users.Commands.UnlockUser;
using TennisReservation.Application.Users.Commands.UpdateUser;
using TennisReservation.Application.Users.Interfaces;
using TennisReservation.Application.Users.Queries.GetLockedUsers;
using TennisReservation.Infrastructure.Postgres;
using TennisReservation.Infrastructure.Postgres.Repositories;
using TennisReservation.Infrastructure.Postgres.Seeding;
using TennisReservation.Infrastructure.Postgres.Services;

namespace TennisReservation.Presentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Регистрация базы данных
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TennisReservationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("TennisReservationDb"));
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.UseLoggerFactory(TennisReservationDbContext.CreateLoggerFactory());
            });

            services.AddScoped<IReadDbContext>(sp =>
                sp.GetRequiredService<TennisReservationDbContext>());

            return services;
        }

        // Регистрация репозиториев
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IUserCredentialsRepository, UsersCredentialsRepository>();
            services.AddScoped<ITennisCourtsRepository, TennisCourtsRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();

            return services;
        }

        // Регистрация сервисов
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<ISeeder, ReservationSeeder>();

            return services;
        }

        // Регистрация обработчиков пользователей
        public static IServiceCollection AddUserHandlers(this IServiceCollection services)
        {
            services.AddScoped<GetAllUsersHandler>();
            services.AddScoped<GetUserByIdHandler>();
            services.AddScoped<GetUserByEmailHandler>();
            services.AddScoped<GetUserWithCredentialsHandler>();
            services.AddScoped<CreateUserWithCredentialsHandler>();
            services.AddScoped<UpdateUserHandler>();
            services.AddScoped<DeleteUserByIdHandler>();
            services.AddScoped<UserService>();
            services.AddScoped<ChangePasswordHandler>();
            services.AddScoped<ChangeRoleHandler>();
            services.AddScoped<LockUserHandler>();
            services.AddScoped<UnlockUserHandler>();
            services.AddScoped<GetLockedUsersHandler>();

            return services;
        }

        // Регистрация обработчиков кортов
        public static IServiceCollection AddTennisCourtHandlers(this IServiceCollection services)
        {
            services.AddScoped<GetAllTennisCourtsHandler>();
            services.AddScoped<GetTennisCourtByIdHandler>();
            services.AddScoped<CreateTennisCourtHandler>();
            services.AddScoped<UpdateTennisCourtHandler>();
            services.AddScoped<DeleteTennisCourtHandler>();
            services.AddScoped<GetAllReservationsByCourtIdHandler>();
            services.AddScoped<GetCourtAvailabilityHandler>();

            return services;
        }

        // Регистрация обработчиков бронирований
        public static IServiceCollection AddReservationHandlers(this IServiceCollection services)
        {
            services.AddScoped<GetAllReservationsHandler>();
            services.AddScoped<GetReservationByIdHandler>();
            services.AddScoped<CreateReservationHandler>();
            services.AddScoped<UpdateReservationHandler>();
            services.AddScoped<DeleteReservationHandler>();
            services.AddScoped<CancelReservationHandler>();
            services.AddHostedService<ReservationStatusUpdater>();
            services.AddScoped<GetAllReservationsByUserIdHandler>();
            services.AddScoped<GetAllReservationByStatusHandler>();
            services.AddScoped<GetAllReservationsByDateHandler>();
            return services;
        }

        public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
        {
            return services
                .AddUserHandlers()
                .AddTennisCourtHandlers()
                .AddReservationHandlers();
        }

        // Регистрация всех сервисов приложения
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddDatabase(configuration)
                .AddRepositories()
                .AddInfrastructureServices()
                .AddApplicationHandlers();
        }
    }
}
