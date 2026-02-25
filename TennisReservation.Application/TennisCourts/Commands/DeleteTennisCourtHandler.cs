using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.TennisCourts.Commands;
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
        public async Task<Result> HandleAsync(DeleteTennisCourtCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existingTennisCourt = await _tennisCourtsRepository.GetByIdWithReservationsAsync(new TennisCourtId(command.Id), cancellationToken);
                if (existingTennisCourt.IsFailure)
                {
                    _logger.LogWarning("Корт с ID {TennisCourtId} не найден", command.Id);
                    return Result.Failure("Корт не найден");
                }
                var tennisCourtToDelete = existingTennisCourt.Value;

                var canDelete = tennisCourtToDelete.CanBeDeleted();
                if (canDelete.IsFailure)
                    return Result.Failure(canDelete.Error);

                var deleteResult = await _tennisCourtsRepository.DeleteAsync(tennisCourtToDelete.Id, cancellationToken);
                if (deleteResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка при удалении корта {TennisCourtId}", command.Id);
                    return Result.Failure(deleteResult.Error);
                }
                _logger.LogInformation("Корт {TennisCourtId} успешно удален", tennisCourtToDelete.Id);
                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении корта {TennisCourtId}", command.Id);
                return Result.Failure("Не удалось удалить корт");
            }
        }
    }
}
