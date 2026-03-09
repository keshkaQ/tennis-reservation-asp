using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Interfaces;
using TennisReservation.Application.Users.Interfaces;

namespace TennisReservation.Application.Users.Commands.ChangePassword
{
    public class ChangePasswordHandler
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserCredentialsRepository _userCredentialsRepository;
        private readonly ILogger<ChangePasswordHandler> _logger;
        public ChangePasswordHandler(IPasswordHasher passwordHasher, IUserCredentialsRepository userCredentialsRepository, ILogger<ChangePasswordHandler> logger)
        {
            _passwordHasher = passwordHasher;
            _userCredentialsRepository = userCredentialsRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = await _userCredentialsRepository.GetWithUserByIdAsync(command.UserId);
                if (credentials.IsFailure || credentials.Value == null)
                    return Result.Failure("Пользователь не найден");
                if (_passwordHasher.Verify(command.NewPassword, credentials.Value.PasswordHash))
                    return Result.Failure("Нельзя изменить пароль на точно такой же");
                var newPasswordHash = _passwordHasher.Hash(command.NewPassword);
                credentials.Value.ChangePassword(newPasswordHash);
                var result = await _userCredentialsRepository.UpdateAsync(credentials.Value);
                if (result.IsFailure)
                    return Result.Failure("Не удалось сменить пароль");

                _logger.LogInformation("Пароль пользователя {UserId} успешно изменен", command.UserId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при смене пароля пользователя {UserId}", command.UserId);
                return Result.Failure("Не удалось сменить пароль");
            }

        }
    }
}
