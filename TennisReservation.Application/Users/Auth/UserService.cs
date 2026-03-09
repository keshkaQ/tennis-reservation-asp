using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Interfaces;
using TennisReservation.Application.Users.Commands.CreateUser;
using TennisReservation.Application.Users.Interfaces;
using TennisReservation.Contracts.Users.Dto;

namespace TennisReservation.Application.Users.Auth
{
    public class UserService
    {
        private readonly CreateUserWithCredentialsHandler _createUserHandler;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IUserCredentialsRepository _credentialsRepository;

        public UserService(CreateUserWithCredentialsHandler createUserHandler,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider,
            IUserCredentialsRepository credentialsRepository,
            ILogger<UserService> logger)
        {
            _createUserHandler = createUserHandler;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _credentialsRepository = credentialsRepository;
            _logger = logger;
        }

        public async Task<Result<UserDto>> Register(CreateUserWithCredentialsCommand command,CancellationToken cancellationToken = default)
        {
            try
            {
                return await _createUserHandler.HandleAsync(command, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя");
                return Result.Failure<UserDto>("Не удалось создать пользователя");
            }
        }

        public async Task<Result<string>> Login(string email, string password)
        {
            try
            {
                var credentialsResult = await _credentialsRepository.GetWithUserByEmailAsync(email);
                if (credentialsResult.IsFailure || credentialsResult.Value == null)
                    return Result.Failure<string>("Неверный email или пароль");

                var credentials = credentialsResult.Value;

                if (!credentials.CanLogin)
                    return Result.Failure<string>($"Аккаунт заблокирован до {credentials.LockedUntil:dd.MM.yyyy HH:mm}");

                var isPasswordValid = _passwordHasher.Verify(password, credentials.PasswordHash);
                if (!isPasswordValid)
                {
                    credentials.RecordFailedAttempt();
                    await _credentialsRepository.UpdateAsync(credentials);
                    var attemptsLeft = 5 - credentials.FailedLoginAttempts;
                    var message = attemptsLeft > 0
                        ? $"Неверный пароль. Осталось попыток: {attemptsLeft}"
                        : $"Аккаунт заблокирован до {credentials.LockedUntil:HH:mm}";
                    return Result.Failure<string>(message);
                }

                credentials.RecordSuccessfulLogin();
                await _credentialsRepository.UpdateAsync(credentials);

                var userDto = new UserLoginDto(
                    credentials.UserId.Value,
                    credentials.User.Email,
                    credentials.Role,
                    credentials.PasswordHash);

                return Result.Success(_jwtProvider.GenerateToken(userDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя {Email}", email);
                return Result.Failure<string>("Произошла ошибка при входе");
            }
        }
    }
}