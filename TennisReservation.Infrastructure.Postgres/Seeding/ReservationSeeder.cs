using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Interfaces;
using TennisReservation.Domain.Enums;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Seeding;

public class ReservationSeeder : ISeeder
{
    private readonly TennisReservationDbContext _dbContext;
    private readonly ILogger<ReservationSeeder> _logger;
    private readonly Faker _faker = new("ru");
    private readonly IPasswordHasher _passwordHasher;

    private const int USERS_COUNT = 50;
    private const int COURTS_COUNT = 10;
    private const int RESERVATIONS_COUNT = 200;

    private readonly (string Name, decimal Price, string Description)[] _courtTemplates = new[]
    {
        ("Центральный корт", 2500m, "Главный корт клуба с профессиональным покрытием"),
        ("Грунтовый корт", 2000m, "Классическое грунтовое покрытие"),
        ("Закрытый корт", 3000m, "Крытый корт с трибунами"),
        ("Тренировочный корт", 1500m, "Идеально для отработки ударов"),
        ("VIP корт", 3500m, "Повышенный комфорт и обслуживание"),
        ("Детский корт", 1000m, "Уменьшенные размеры для детей"),
        ("Профессиональный корт", 2800m, "Сертифицирован для турниров"),
        ("Любительский корт", 1200m, "Для игры в удовольствие"),
        ("Утренний корт", 800m, "Специальная цена для утренних игр"),
        ("Вечерний корт", 1800m, "С отличным освещением")
    };

    public ReservationSeeder(TennisReservationDbContext dbContext, IPasswordHasher passwordHasher, ILogger<ReservationSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Начинаем заполнение базы данных");

        try
        {
            await SeedData();
            _logger.LogInformation("Заполнение базы данных закончено");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при заполнении базы");
            throw;
        }
    }

    private async Task SeedData()
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await ClearDatabase();
            await SeedUsersAsync();
            await SeedCourtsAsync();
            await SeedReservationsAsync();

            await transaction.CommitAsync();
            _logger.LogInformation("Заполнение прошло успешно");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task ClearDatabase()
    {
        _logger.LogInformation("Очищаем базу данных");

        _dbContext.Reservations.RemoveRange(_dbContext.Reservations);
        _dbContext.UserCredentials.RemoveRange(_dbContext.UserCredentials);
        _dbContext.Users.RemoveRange(_dbContext.Users);
        _dbContext.TennisCourts.RemoveRange(_dbContext.TennisCourts);

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Очищение базы закончено");
    }

    private async Task SeedUsersAsync()
    {
        _logger.LogInformation("Заполняем пользователей");

        var users = new List<User>();
        var usedEmails = new HashSet<string>();

        var adminResult = User.Create(
            "admin",          
            "admin",         
            "admin@email.com",
            "+79990000001"     
        );
        usedEmails.Add(adminResult.Value.Email);

        var adminPasswordHash = _passwordHasher.Hash("admin");
        var adminCredentialsResult = UserCredentials.Create(
            adminResult.Value.Id,
            adminPasswordHash,
            UserRole.Admin
        );

        if (adminCredentialsResult.IsFailure)
        {
            _logger.LogError("Ошибка при создании учетных данных администратора: {Error}", adminCredentialsResult.Error);
            throw new Exception(adminCredentialsResult.Error);
        }

        adminResult.Value.SetCredentials(adminCredentialsResult.Value);
        users.Add(adminResult.Value);
        _logger.LogInformation("Создан администратор: admin@email.com");

        var userResult = User.Create(
            "user",           
            "user",          
            "user@email.com",  
            "+79990000002"    
        );

        var user = userResult.Value;
        usedEmails.Add(user.Email); 

        var userPasswordHash = _passwordHasher.Hash("user");
        var userCredentialsResult = UserCredentials.Create(
            user.Id,
            userPasswordHash,
            UserRole.User
        );

        if (userCredentialsResult.IsFailure)
        {
            _logger.LogError("Ошибка при создании учетных данных пользователя: {Error}", userCredentialsResult.Error);
            throw new Exception(userCredentialsResult.Error);
        }

        user.SetCredentials(userCredentialsResult.Value);
        users.Add(user);
        _logger.LogInformation("Создан пользователь: user@email.com");

        // Общий пароль для всех тестовых пользователей
        const string testPassword = "Test123!";

        // Хешируем пароль один раз для всех пользователей
        string passwordHash = _passwordHasher.Hash(testPassword);

        var userFaker = new Faker<User>("ru")
            .CustomInstantiator(f =>
            {
                var firstName = f.Name.FirstName();
                var lastName = f.Name.LastName();
                var email = f.Internet.Email(firstName, lastName).ToLower();

                while (usedEmails.Contains(email))
                {
                    email = f.Internet.Email(firstName, lastName, f.Random.String2(2)).ToLower();
                }
                usedEmails.Add(email);

                var phoneNumber = f.Phone.PhoneNumber("+79#########");

                var userResult = User.Create(firstName, lastName, email, phoneNumber);

                if (userResult.IsFailure)
                    throw new Exception(userResult.Error);

                return userResult.Value;
            });

        var generatedUsers = userFaker.Generate(USERS_COUNT);

        for (int i = 0; i < generatedUsers.Count; i++)
        {
            var generatedUser = generatedUsers[i];
            var credentialsResult = UserCredentials.Create(
                generatedUser.Id,
                passwordHash,
                UserRole.User
            );

            if (credentialsResult.IsFailure)
            {
                _logger.LogWarning("Ошибка при создании учетных данных: {Error}", credentialsResult.Error);
                continue;
            }

            generatedUser.SetCredentials(credentialsResult.Value);
            users.Add(generatedUser);
        }

        await _dbContext.Users.AddRangeAsync(users);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Добавлено {Count} пользователей (2 конкретных + {GeneratedCount} сгенерированных)",
            users.Count, generatedUsers.Count);
    }

    private async Task SeedCourtsAsync()
    {
        _logger.LogInformation("Заполняем теннисные корты");

        var courts = new List<TennisCourt>();

        for (int i = 0; i < COURTS_COUNT && i < _courtTemplates.Length; i++)
        {
            var template = _courtTemplates[i];

            var courtResult = TennisCourt.Create(
                template.Name,
                template.Price,
                template.Description
            );

            if (courtResult.IsFailure)
            {
                _logger.LogError("Ошибка при создании корта {CourtName}: {Error}", template.Name, courtResult.Error);
                continue;
            }

            courts.Add(courtResult.Value);
        }

        if (courts.Any())
        {
            await _dbContext.TennisCourts.AddRangeAsync(courts);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Добавлено {Count} теннисных кортов", courts.Count);
        }
    }

    private async Task SeedReservationsAsync()
    {
        _logger.LogInformation("Заполняем бронирования");

        var users = await _dbContext.Users.ToListAsync();
        var courts = await _dbContext.TennisCourts.ToListAsync();

        if (!users.Any() || !courts.Any())
        {
            _logger.LogWarning("Нет пользователей или кортов для бронирования");
            return;
        }

        var reservations = new List<Reservation>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < RESERVATIONS_COUNT; i++)
        {
            var user = _faker.PickRandom(users);
            var court = _faker.PickRandom(courts);

            var daysFromNow = _faker.Random.Int(1, 30);
            var startHour = _faker.Random.Int(8, 21);
            var durationHours = _faker.Random.Int(1, 3);

            var startTime = now.Date.AddDays(daysFromNow).AddHours(startHour);
            var endTime = startTime.AddHours(durationHours);
            var totalCost = durationHours * court.HourlyRate;

            var reservationResult = Reservation.Create(
                court.Id,
                user.Id,
                startTime,
                endTime,
                totalCost
            );

            if (reservationResult.IsSuccess)
            {
                reservations.Add(reservationResult.Value);
            }
        }

        await _dbContext.Reservations.AddRangeAsync(reservations);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Добавлено {Count} бронирований", reservations.Count);
    }
}