using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.TennisCourts;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Reservations.Commands
{
    public class UpdateReservationHandler
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ITennisCourtsRepository _courtsRepository;
        private readonly ILogger<UpdateReservationHandler> _logger;

        public UpdateReservationHandler(IReservationRepository reservationRepository, ITennisCourtsRepository courtsRepository, ILogger<UpdateReservationHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _courtsRepository = courtsRepository;
            _logger = logger;
        }
        public async Task<Result<ReservationDto>> HandleAsync(UpdateReservationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existingReservation = await _reservationRepository.GetByIdAsync(new ReservationId(command.Id),cancellationToken);
                if (existingReservation.IsFailure)
                {
                    _logger.LogWarning("Бронирование с ID {ReservationId} не найден", command.Id);
                    return Result.Failure<ReservationDto>("Бронирование не найдено");
                }
                var court = await _courtsRepository.GetByIdAsync(new TennisCourtId(command.TennisCourtId), cancellationToken);

                if (court.IsFailure)
                    return Result.Failure<ReservationDto>("Корт не найден");
                var hours = (decimal)(command.EndTime - command.StartTime).TotalHours;
                var totalCost = hours * court.Value.HourlyRate;

                var reservationToUpdate = existingReservation.Value;

                var updateResult = reservationToUpdate.Update(
                    new TennisCourtId(command.TennisCourtId),
                    new UserId(command.UserId),
                    command.StartTime,
                    command.EndTime, totalCost
                    );
                if(updateResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка валидации при обновлении бронирования" +
                        "{ReservationId}: {Error}",command.Id, updateResult.Error);
                    return Result.Failure<ReservationDto>(updateResult.Error);
                }
                var saveResult = await _reservationRepository.UpdateAsync(reservationToUpdate, cancellationToken);
                if(saveResult.IsFailure)
                {
                    _logger.LogError(
                       "Не удалось сохранить бронирование {ReservationId} в БД",
                       command.Id);
                    return Result.Failure<ReservationDto>(saveResult.Error);
                }
                var dto = new ReservationDto(reservationToUpdate.Id.Value,
                    reservationToUpdate.TennisCourtId.Value,
                    reservationToUpdate.UserId.Value,
                    reservationToUpdate.StartTime,
                    reservationToUpdate.EndTime,
                    reservationToUpdate.TotalCost,
                    reservationToUpdate.Status);

                _logger.LogInformation(
             "Бронирование {ReservationId} успешно обновлено",
             reservationToUpdate.Id.Value);

                return Result.Success(dto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении бронирования {ReservationId}", command.Id);
                return Result.Failure<ReservationDto>("Не удалось обновить бронирование");
            }
        }
    }
}
