using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.Reservations.Command;

namespace TennisReservation.Application.Reservations.Commands
{
    public class DeleteReservationHandler
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<DeleteReservationHandler> _logger;
        public DeleteReservationHandler(IReservationRepository reservationRepository, ILogger<DeleteReservationHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(DeleteReservationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existingReservation = await _reservationRepository.GetByIdAsync(new Domain.Models.ReservationId(command.Id),cancellationToken);
                if(existingReservation.IsFailure)
                {
                    _logger.LogWarning("Бронирование с ID {ReservationId} не найдено", command.Id);
                    return Result.Failure($"Бронирование с {command.Id} не найдено");
                }
                var reservationToDelete = existingReservation.Value;

                // Проверка прав обычный пользователь может удалить только свою бронь
                if (!command.IsAdmin && command.RequestingUserId != reservationToDelete.UserId.Value)
                {
                    _logger.LogWarning("Пользователь {UserId} попытался удалить чужое бронирование {ReservationId}",
                        command.RequestingUserId, command.Id);
                    return Result.Failure("Нет прав для удаления этого бронирования");
                }

                var deleteResult = await _reservationRepository.DeleteAsync(reservationToDelete.Id, cancellationToken);
                if(deleteResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка при удалении бронирования {ReservationId}", command.Id);
                    return Result.Failure(deleteResult.Error);
                }
                _logger.LogInformation("Бронирование {ReservationId} успешно удалено", reservationToDelete.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении бронирования {ReservationId}", command.Id);
                return Result.Failure("Не удалось удалить бронирование");
            }
        }
    }
}
