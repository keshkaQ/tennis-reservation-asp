using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.TennisCourts.Commands
{
    public class UpdateTennisCourtHandler
    {
        private readonly ILogger<UpdateTennisCourtHandler> _logger;
        private readonly ITennisCourtsRepository _tennisCourtsRepository;
        public UpdateTennisCourtHandler(ITennisCourtsRepository tennisCourtsRepository, ILogger<UpdateTennisCourtHandler> logger)
        {
            _tennisCourtsRepository = tennisCourtsRepository;
            _logger = logger;
        }

        public async Task<Result<TennisCourtDto>> HandleAsync(Guid id,UpdateTennisCourtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingCourt = await _tennisCourtsRepository.GetByIdWithReservationsAsync(new TennisCourtId(id), cancellationToken);
                if (existingCourt.IsFailure)
                {
                    _logger.LogWarning("Корт с ID {TennisCourtId} не найден", id);
                    return Result.Failure<TennisCourtDto>("Корт не найден");
                }
                var tennisCourtToUpdate = existingCourt.Value;

                var updateResult = tennisCourtToUpdate.Update(
                    request.Name,
                    request.HourlyRate,
                    request.Description);

                if (updateResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Ошибка валидации при обновлении корта {TennisCourtId}: {Error}", id, updateResult.Error);
                    return Result.Failure<TennisCourtDto>(updateResult.Error);
                }
                var saveResult = await _tennisCourtsRepository.UpdateAsync(tennisCourtToUpdate, cancellationToken);
                if (saveResult.IsFailure)
                {
                    _logger.LogError(
                        "Не удалось сохранить корт {TennisCourtId} в БД",
                        id);
                    return Result.Failure<TennisCourtDto>(saveResult.Error);
                }
                var dto = new TennisCourtDto(
                    tennisCourtToUpdate.Id.Value,
                    tennisCourtToUpdate.Name,
                    tennisCourtToUpdate.HourlyRate,
                    tennisCourtToUpdate.Description
                    );

                _logger.LogInformation(
               "Корт {TennisCourtId} успешно обновлен",
               tennisCourtToUpdate.Id.Value);

                return Result.Success(dto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении корта {TennisCourtId}", id);
                return Result.Failure<TennisCourtDto>("Не удалось обновить корт");
            }
        }
    }
}
