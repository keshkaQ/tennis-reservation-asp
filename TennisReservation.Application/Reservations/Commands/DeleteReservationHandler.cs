using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.Reservations.Queries;

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

        public async Task<Result> HandleAsync(DeleteReservationByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var existingReservation = await _reservationRepository.GetByIdAsync(new Domain.Models.ReservationId(query.Id),cancellationToken);
                if(existingReservation.IsFailure)
                {
                    _logger.LogWarning("Бронирование с ID {ReservationId} не найдено", query.Id);
                    return Result.Failure($"Бронирование с {query.Id} не найдено");
                }
                var reservationToDelete = existingReservation.Value;
                var deleteResult = await _reservationRepository.DeleteAsync(reservationToDelete.Id, cancellationToken);
                if(deleteResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка при удалении бронирования {ReservationId}", query.Id);
                    return Result.Failure(deleteResult.Error);
                }
                _logger.LogInformation("Бронирование {ReservationId} успешно удалено", reservationToDelete.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении бронирования {ReservationId}", query.Id);
                return Result.Failure("Не удалось удалить бронирование");
            }
        }
    }
}
