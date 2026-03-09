using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Reservations.Interfaces;
using TennisReservation.Application.TennisCourts.Interfaces;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Reservations.Commands.CreateReservation
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
                var startTime = DateTime.SpecifyKind(command.StartTime, DateTimeKind.Utc);
                var endTime = DateTime.SpecifyKind(command.EndTime, DateTimeKind.Utc);

                var court = await _courtsRepository.GetByIdAsync(
                    new TennisCourtId(command.TennisCourtId),
                    cancellationToken);
                if (court.IsFailure)
                    return Result.Failure<ReservationDto>("Корт не найден");

                var isAvailable = await _reservationRepository.CheckAvailabilityAsync(
                    command.TennisCourtId,
                    startTime,
                    endTime,
                    cancellationToken: cancellationToken);
                if (!isAvailable)
                    return Result.Failure<ReservationDto>("Корт уже забронирован на это время");

                var hours = (endTime - startTime).TotalHours;
                var totalCost = (decimal)hours * court.Value.HourlyRate;

                var reservationResult = Reservation.Create(
                    new TennisCourtId(command.TennisCourtId),
                    new UserId(command.UserId),
                    startTime,
                    endTime,
                    totalCost);

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
