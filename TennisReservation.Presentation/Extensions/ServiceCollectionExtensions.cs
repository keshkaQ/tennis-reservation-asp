using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Auth;
using TennisReservation.Application.Database;
using TennisReservation.Application.Interfaces;
using TennisReservation.Application.Reservations;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Application.Reservations.Queries;
using TennisReservation.Application.Statictics;
using TennisReservation.Application.TennisCourts;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Application.Users;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
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
            services.AddScoped<DeleteUserHandler>();
            services.AddScoped<UserService>();
            services.AddScoped<GetUserWithCredentialsByEmailHandler>();

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

            return services;
        }

        // Регистрация статистики
        public static IServiceCollection AddStatisticsHandlers(this IServiceCollection services)
        {
            services.AddScoped<GetStatisticsHandler>();

            return services;
        }

        // Комбинированный метод для регистрации всех обработчиков
        public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
        {
            return services
                .AddUserHandlers()
                .AddTennisCourtHandlers()
                .AddReservationHandlers()
                .AddStatisticsHandlers();
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
