using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.Users.Commands;

namespace TennisReservation.Application.Users.Commands
{
    public class LockUserHandler
    {
        private readonly IUserCredentialsRepository _userCredentialsRepository;
        private readonly ILogger<LockUserHandler> _logger;

        public LockUserHandler(IUserCredentialsRepository userCredentialsRepository, ILogger<LockUserHandler> logger)
        {
            _userCredentialsRepository = userCredentialsRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(LockUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = await _userCredentialsRepository.GetWithUserByIdAsync(command.UserId);
                if (credentials.IsFailure || credentials.Value == null)
                    return Result.Failure("Пользователь не найден");
                if (command.Lock <= DateTime.UtcNow)
                    return Result.Failure("Дата блокировки должна быть в будущем");

                credentials.Value.LockUntil(command.Lock);
                var result = await _userCredentialsRepository.UpdateAsync(credentials.Value);
                if (result.IsFailure)
                    return Result.Failure("Не удалось заблокировать пользователя");

                _logger.LogInformation("Блокировка пользователя произошла {UserId} успешна", command.UserId);
                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при блокировке пользователя {UserId}", command.UserId);
                return Result.Failure("Не удалось поставить блокировку");
            }
          
        }
    }
}
