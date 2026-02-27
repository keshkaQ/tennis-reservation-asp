using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Interfaces;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Contracts.Users.Queries;

namespace TennisReservation.Application.Auth
{
    public class UserService
    {
        private readonly CreateUserWithCredentialsHandler _createUserHandler;
        private readonly GetUserWithCredentialsByEmailHandler _getUserWithCredentialsByEmailHandler;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public UserService(CreateUserWithCredentialsHandler createUserHandler,
            GetUserWithCredentialsByEmailHandler getUserWithCredentialsByEmailHandler, 
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider,
            ILogger<UserService> logger)
        {
            _createUserHandler = createUserHandler;
            _getUserWithCredentialsByEmailHandler = getUserWithCredentialsByEmailHandler;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _logger = logger;
        }

        public async Task<Result<UserDto>> Register(CreateUserCommand command,CancellationToken cancellationToken = default)
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

        public async Task<string?> Login(string email, string password)
        {
            var userResult = await _getUserWithCredentialsByEmailHandler.HandleAsync(
                new GetUserWithCredentialsByEmailQuery(email),
                CancellationToken.None);

            if (userResult.IsFailure)
            {
                _logger.LogWarning($"Ошибка при получении пользователя по email {email} : {userResult.Error}");
                return null;
            }

            var isPasswordValid = _passwordHasher.Verify(password, userResult.Value.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.LogWarning($"Введен неверный пароль для пользователя {userResult.Value.UserId}");
                return null;
            }

            return _jwtProvider.GenerateToken(userResult.Value);
        }
    }
}