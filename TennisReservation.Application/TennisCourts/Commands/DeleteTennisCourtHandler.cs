using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.TennisCourts.Queries;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.TennisCourts.Commands
{
    public class DeleteTennisCourtHandler
    {
        private readonly ITennisCourtsRepository _tennisCourtsRepository;
        private readonly ILogger<DeleteTennisCourtHandler> _logger;
        public DeleteTennisCourtHandler(ITennisCourtsRepository tennisCourtsRepository, ILogger<DeleteTennisCourtHandler> logger)
        {
            _tennisCourtsRepository = tennisCourtsRepository;
            _logger = logger;
        }
        public async Task<Result> HandleAsync(DeleteTennisCourtByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var existingTennisCourt = await _tennisCourtsRepository.GetByIdWithReservationsAsync(new TennisCourtId(query.Id), cancellationToken);
                if (existingTennisCourt.IsFailure)
                {
                    _logger.LogWarning("Корт с ID {TennisCourtId} не найден", query.Id);
                    return Result.Failure("Корт не найден");
                }
                var tennisCourtToDelete = existingTennisCourt.Value;

                if (tennisCourtToDelete.Reservations?.Any() == true)
                {
                    _logger.LogWarning("Невозможно удалить корт {TennisCourtId} - есть активные брони", query.Id);
                    return Result.Failure("Невозможно удалить корт с существующими бронями");
                }
                var deleteResult = await _tennisCourtsRepository.DeleteAsync(tennisCourtToDelete.Id, cancellationToken);
                if (deleteResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка при удалении корта {TennisCourtId}", query.Id);
                    return Result.Failure(deleteResult.Error);
                }
                _logger.LogInformation("Корт {TennisCourtId} успешно удален", tennisCourtToDelete.Id);
                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении корта {TennisCourtId}", query.Id);
                return Result.Failure("Не удалось удалить корт");
            }
        }
    }
}
