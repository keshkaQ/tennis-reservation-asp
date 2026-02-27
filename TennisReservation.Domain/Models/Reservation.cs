using CSharpFunctionalExtensions;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Domain.Models
{
    public record ReservationId(Guid Value);
    public class Reservation
    {
        private Reservation() { }

        private Reservation(ReservationId id, UserId userId, TennisCourtId courtId,
            DateTime startTime, DateTime endTime, decimal totalCost, ReservationStatus status, DateTime createdAt)
        {
            Id = id;
            UserId = userId;
            TennisCourtId = courtId;
            StartTime = startTime;
            EndTime = endTime;
            TotalCost = totalCost;
            Status = status;
            CreatedAt = createdAt;
        }

        public ReservationId Id { get; }
        public TennisCourtId TennisCourtId { get; private set; }
        public UserId UserId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public decimal TotalCost { get; private set; }
        public ReservationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public virtual User User { get; private set; } = null!;
        public virtual TennisCourt TennisCourt { get; private set; } = null!;

        public static Result<Reservation> Create(
            TennisCourtId tennisCourtId,
            UserId userId,
            DateTime startTime,
            DateTime endTime,
            decimal totalCost)
        {
            // Валидация
            var validationResult = ValidateGuid(tennisCourtId, userId)
                 .Bind(() => ValidateTime(startTime, endTime))
                 .Bind(() => ValidateCost(totalCost));

            if (validationResult.IsFailure)
                return Result.Failure<Reservation>(validationResult.Error);

            // Конвертируем время в UTC
            startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

            return new Reservation(
                new ReservationId(Guid.NewGuid()),
                userId,
                tennisCourtId,
                startTime,
                endTime,
                totalCost,
                ReservationStatus.Booked,
                DateTime.UtcNow);
        }


        public Result Update(
            TennisCourtId tennisCourtId,
            UserId userId,
            DateTime startTime,
            DateTime endTime,
            decimal totalCost)
        {
            var validationResult = ValidateGuid(tennisCourtId, userId)
                 .Bind(() => ValidateTime(startTime, endTime))
                 .Bind(() => ValidateCost(totalCost));

            if (validationResult.IsFailure)
                return validationResult;

            TennisCourtId = tennisCourtId;
            UserId = userId;
            StartTime = DateTime.SpecifyKind(startTime,DateTimeKind.Utc);
            EndTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
            TotalCost = totalCost;

            return Result.Success();
        }

        // Метод для обновления только статуса
        public Result UpdateStatus(ReservationStatus newStatus)
        {
            // Бизнес-правила для смены статуса
            if (Status == ReservationStatus.Cancelled && newStatus != ReservationStatus.Cancelled)
                return Result.Failure("Нельзя восстановить отмененное бронирование");

            if (Status == ReservationStatus.Completed && newStatus != ReservationStatus.Completed)
                return Result.Failure("Нельзя изменить завершенное бронирование");

            Status = newStatus;
            return Result.Success();
        }

        // Метод для пересчета стоимости (если изменилось время или корт)
        public Result RecalculateCost(decimal hourlyRate)
        {
            var hours = (decimal)(EndTime - StartTime).TotalHours;
            var newTotalCost = hours * hourlyRate;

            if (newTotalCost <= 0)
                return Result.Failure("Не удалось рассчитать стоимость");

            TotalCost = newTotalCost;
            return Result.Success();
        }

        // Валидация
        private static Result ValidateGuid(TennisCourtId tennisCourtId, UserId userId)
        {
            if (tennisCourtId.Value == Guid.Empty)
                return Result.Failure("Id корта не может быть пустым");
            if (userId.Value == Guid.Empty)
                return Result.Failure("Id пользователя не может быть пустым");
            return Result.Success();
        }

        private static Result ValidateTime(DateTime startTime, DateTime endTime)
        {
            if (startTime < DateTime.UtcNow.AddMinutes(-5))
                return Result.Failure("Дата начала не может быть в прошлом");
            if (endTime < DateTime.UtcNow)
                return Result.Failure("Дата окончания не может быть в прошлом");
            if (endTime <= startTime)
                return Result.Failure("Время окончания должно быть позже времени начала");
            if ((endTime - startTime).TotalMinutes < 30)
                return Result.Failure("Минимальная длительность бронирования - 30 минут");
            if ((endTime - startTime).TotalHours > 24)
                return Result.Failure("Максимальная длительность бронирования - 24 часа");
            return Result.Success();
        }

        private static Result ValidateCost(decimal totalCost)
        {
            if (totalCost < 0)
                return Result.Failure("Стоимость не может быть отрицательной");
            return Result.Success();
        }
    }
}