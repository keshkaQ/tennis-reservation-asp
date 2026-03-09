using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Reservations.Interfaces;

namespace TennisReservation.Application.Reservations.Commands.CancelReservation
{
    public class CancelReservationHandler
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<CancelReservationHandler> _logger;

        public CancelReservationHandler(IReservationRepository reservationRepository, ILogger<CancelReservationHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(CancelReservationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existingReservation = await _reservationRepository.GetByIdAsync(new Domain.Models.ReservationId(command.Id), cancellationToken);
                if (existingReservation.IsFailure)
                {
                    _logger.LogWarning("Бронирование с ID {ReservationId} не найдено", command.Id);
                    return Result.Failure($"Бронирование с {command.Id} не найдено");
                }
                var reservationToCancel = existingReservation.Value;
                reservationToCancel.Cancel();
                var saveResult = await _reservationRepository.UpdateAsync(reservationToCancel, cancellationToken);
                if (saveResult.IsFailure)
                    return Result.Failure("Не удалось сохранить отмену бронирования");

                _logger.LogInformation("Бронирование {ReservationId} успешно отменено", command.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отмене бронирования {ReservationId}", command.Id);
                return Result.Failure("Не удалось отменить бронирование");
            }
        }
    }
}
