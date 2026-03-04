using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.Users.Commands;

namespace TennisReservation.Application.Users.Commands
{
    public class UnlockUserHandler
    {
        private readonly IUserCredentialsRepository _userCredentialsRepository;
        private readonly ILogger<UnlockUserHandler> _logger;

        public UnlockUserHandler(IUserCredentialsRepository userCredentialsRepository, ILogger<UnlockUserHandler> logger)
        {
            _userCredentialsRepository = userCredentialsRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(UnlockUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = await _userCredentialsRepository.GetWithUserByIdAsync(command.UserId);
                if (credentials.IsFailure || credentials.Value == null)
                    return Result.Failure("Пользователь не найден");
                if(!credentials.Value.IsLocked())
                    return Result.Failure("Пользователь не имеет блокировку");

                credentials.Value.ResetLockout();
                var result = await _userCredentialsRepository.UpdateAsync(credentials.Value);
                if (result.IsFailure)
                    return Result.Failure("Не удалось разблокировать пользователя");

                _logger.LogInformation("Разблокировка пользователя произошла {UserId} успешна", command.UserId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при разблокировке пользователя {UserId}", command.UserId);
                return Result.Failure("Не удалось снять блокировку");
            }
        }
    }
}
