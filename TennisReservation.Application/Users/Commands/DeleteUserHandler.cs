using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.Users.Queries;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users.Commands
{
    public class DeleteUserHandler
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<DeleteUserHandler> _logger;
        public DeleteUserHandler(IUsersRepository usersRepository, ILogger<DeleteUserHandler> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(DeleteUserByIdQuery query,CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _usersRepository.GetByIdAsync(new UserId(query.Id),cancellationToken);
                if(existingUser.IsFailure)
                {
                    _logger.LogWarning("Пользователь с ID {UserId} не найден", query.Id);
                    return Result.Failure("Пользователь не найден");
                }
                var userToDelete = existingUser.Value;
                if (userToDelete.Reservations?.Any() == true)
                {
                    _logger.LogWarning(
                        "Невозможно удалить пользователя {UserId} - есть активные брони",
                        query.Id);
                    return Result.Failure("Невозможно удалить пользователя с активными бронями");
                }
                var deleteResult = await _usersRepository.DeleteWithCredentialsAsync(userToDelete.Id, cancellationToken);
                if (deleteResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка при удалении пользователя {UserId}", query.Id);
                    return Result.Failure(deleteResult.Error);
                }
                _logger.LogInformation(
                   "Пользователь {UserId} успешно удален",
                   userToDelete.Id);

                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя {UserId}", query.Id);
                return Result.Failure("Не удалось удалить пользователя");
            }
        }
    }
}
