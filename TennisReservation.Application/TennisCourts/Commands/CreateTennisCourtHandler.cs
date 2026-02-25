using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.TennisCourts.Commands
{
    public class CreateTennisCourtHandler
    {
        private readonly ITennisCourtsRepository _tennisCourtsRepository;
        private readonly ILogger<CreateTennisCourtHandler> _logger;
        public CreateTennisCourtHandler(ITennisCourtsRepository tennisCourtsRepository, ILogger<CreateTennisCourtHandler> logger)
        {
            _tennisCourtsRepository = tennisCourtsRepository;
            _logger = logger;
        }

        public async Task<Result<TennisCourtDto>> HandleAsync(CreateTennisCourtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tennisCourtResult = TennisCourt.Create(request.Name, request.HourlyRate, request.Description);
                if (tennisCourtResult.IsFailure)
                    return Result.Failure<TennisCourtDto>(tennisCourtResult.Error);

                var tennisCourt = tennisCourtResult.Value;
                var saveResult = await _tennisCourtsRepository.CreateAsync(tennisCourt, cancellationToken);
                if (saveResult.IsSuccess)
                {
                    var dto = new TennisCourtDto(
                        tennisCourt.Id.Value,
                        tennisCourt.Name,
                        tennisCourt.HourlyRate,
                        tennisCourt.Description);
                    _logger.LogInformation("Корт {TennisCourtId} успешно создан", tennisCourt.Id.Value);
                    return Result.Success(dto);
                }
                return Result.Failure<TennisCourtDto>(saveResult.Error);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании корта");
                return Result.Failure<TennisCourtDto>("Не удалось создать корт");
            }
        }
    }
}
