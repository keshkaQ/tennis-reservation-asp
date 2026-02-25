using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Interfaces;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Domain.Models;


namespace TennisReservation.Application.Users.Commands
{
    public class CreateUserWithCredentialsHandler
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<CreateUserWithCredentialsHandler> _logger;
        private readonly IPasswordHasher _passwordHasher;
        public CreateUserWithCredentialsHandler(
            IUsersRepository usersRepository,
            IPasswordHasher passwordHasher,
            ILogger<CreateUserWithCredentialsHandler> logger)
        {
            _usersRepository = usersRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<UserDto>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var userResult = User.Create(command.FirstName, command.LastName, command.Email, command.PhoneNumber);
                if (userResult.IsFailure)
                    return Result.Failure<UserDto>(userResult.Error);

                var user = userResult.Value;

                var passwordHash = _passwordHasher.Hash(command.Password);

                var credentialsResult = UserCredentials.Create(user.Id,passwordHash,command.Role);

                if (credentialsResult.IsFailure)
                    return Result.Failure<UserDto>(credentialsResult.Error);

                user.SetCredentials(credentialsResult.Value);

                var saveResult = await _usersRepository.CreateWithCredentialsAsync(user, cancellationToken);

                if (saveResult.IsSuccess)
                {
                    var dto = new UserDto(
                        user.Id.Value,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.PhoneNumber,
                        user.RegistrationDate,
                        user.Reservations?.Count ?? 0
                    );

                    _logger.LogInformation("Пользователь {UserId} успешно создан", user.Id.Value);
                    return Result.Success(dto);
                }

                return Result.Failure<UserDto>(saveResult.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя");
                return Result.Failure<UserDto>("Не удалось создать пользователя");
            }
        }
    }
}
    