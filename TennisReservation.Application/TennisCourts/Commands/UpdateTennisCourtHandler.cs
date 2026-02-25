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

        public async Task<Result<TennisCourtDto>> HandleAsync(UpdateTennisCourtCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existingCourt = await _tennisCourtsRepository.GetByIdWithReservationsAsync(new TennisCourtId(command.Id), cancellationToken);
                if (existingCourt.IsFailure)
                {
                    _logger.LogWarning("Корт с ID {TennisCourtId} не найден", command.Id);
                    return Result.Failure<TennisCourtDto>("Корт не найден");
                }
                var tennisCourtToUpdate = existingCourt.Value;

                var updateResult = tennisCourtToUpdate.Update(
                    command.Name,
                    command.HourlyRate,
                    command.Description);

                if (updateResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка валидации при обновлении корта {TennisCourtId}: {Error}", command.Id, updateResult.Error);
                    return Result.Failure<TennisCourtDto>(updateResult.Error);
                }
                var saveResult = await _tennisCourtsRepository.UpdateAsync(tennisCourtToUpdate, cancellationToken);
                if (saveResult.IsFailure)
                {
                    _logger.LogError("Не удалось сохранить корт {TennisCourtId} в БД",command.Id);
                    return Result.Failure<TennisCourtDto>(saveResult.Error);
                }
                var dto = new TennisCourtDto(
                    tennisCourtToUpdate.Id.Value,
                    tennisCourtToUpdate.Name,
                    tennisCourtToUpdate.HourlyRate,
                    tennisCourtToUpdate.Description
                    );

                _logger.LogInformation("Корт {TennisCourtId} успешно обновлен",tennisCourtToUpdate.Id.Value);

                return Result.Success(dto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении корта {TennisCourtId}", command.Id);
                return Result.Failure<TennisCourtDto>("Не удалось обновить корт");
            }
        }
    }
}
