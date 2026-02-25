using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.TennisCourts;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Reservations.Commands
{
    public class CreateReservationHandler
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ITennisCourtsRepository _courtsRepository;
        private readonly ILogger<CreateReservationHandler> _logger;
        public CreateReservationHandler(IReservationRepository reservationRepository,
            ITennisCourtsRepository courtsRepository,
            ILogger<CreateReservationHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _courtsRepository = courtsRepository;
            _logger = logger;
        }

        public async Task<Result<ReservationDto>> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var court = await _courtsRepository.GetByIdAsync(
                    new TennisCourtId(command.TennisCourtId),
                    cancellationToken);

                if (court.IsFailure)
                    return Result.Failure<ReservationDto>("Корт не найден");

                var hours = (command.EndTime - command.StartTime).TotalHours;
                var totalCost = (decimal)hours * court.Value.HourlyRate;

                var reservationResult = Reservation.Create(new TennisCourtId(command.TennisCourtId), new UserId(command.UserId), command.StartTime, command.EndTime, totalCost);
                if (reservationResult.IsFailure)
                    return Result.Failure<ReservationDto>(reservationResult.Error);

                var reservation = reservationResult.Value;
                var saveResult = await _reservationRepository.CreateAsync(reservation, cancellationToken);
                if (saveResult.IsSuccess)
                {
                    var dto = new ReservationDto(
                        reservation.Id.Value,
                        reservation.TennisCourtId.Value,
                        reservation.UserId.Value,
                        reservation.StartTime,
                        reservation.EndTime,
                        reservation.TotalCost,
                        reservation.Status
                       );
                    _logger.LogInformation("Бронирование {ReservationId} успешно создано", reservation.Id.Value);
                    return Result.Success(dto);
                }
                return Result.Failure<ReservationDto>(saveResult.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании бронирования");
                return Result.Failure<ReservationDto>("Не удалось создать бронирование");
            }
        }
    }
}
