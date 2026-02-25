using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using TennisReservation.Domain.Constants;

namespace TennisReservation.Domain.Models
{
    public record TennisCourtId(Guid Value);
    public class TennisCourt
    {
        private TennisCourt() { }
        public TennisCourt(TennisCourtId id, string name, decimal hourlyRate, string description)
        {
            Id = id;
            Name = name;
            HourlyRate = hourlyRate;
            Description = description;
            _reservations = [];
        }

        public TennisCourtId Id { get; }
        public string Name { get; private set; }
        public decimal HourlyRate { get; private set; }
        public string Description { get; private set; }

        private List<Reservation> _reservations;
        public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();
        public static Result<TennisCourt> Create(string name, decimal hourlyRate, string description)
        {
            name = name?.Trim() ?? "";
            description = description?.Trim() ?? "";

            var validationResult = ValidateNames(name, description)
                  .Bind(() => ValidateHourlyRate(hourlyRate));

            if (validationResult.IsFailure)
                return Result.Failure<TennisCourt>(validationResult.Error);

            return new TennisCourt(
                new TennisCourtId(Guid.NewGuid()),
                name,
                hourlyRate,
                description);
        }

        public Result Update(string name,decimal hourlyRate,string description)
        {
            name = name?.Trim() ?? "";
            description = description?.Trim() ?? "";

            var validationResult = ValidateNames(name, description)
                 .Bind(() => ValidateHourlyRate(hourlyRate));

            if (validationResult.IsFailure)
                return validationResult;
            Name = name;
            HourlyRate = hourlyRate;
            Description = description;
            return Result.Success();
        }
        public Result CanBeDeleted()
        {
            if (_reservations.Any())
                return Result.Failure("Невозможно удалить корт с существующими бронями");
            return Result.Success();
        }

        private static Result ValidateNames(string name,string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<TennisCourt>("Название корта не может быть пустым");
            if (name.Length < LengthConstants.LENGTH2 || name.Length > LengthConstants.LENGTH50)
                return Result.Failure<TennisCourt>("Название корта должно быть от 2 до 50 символов");

            if (string.IsNullOrWhiteSpace(description))
                return Result.Failure<TennisCourt>("Описание корта не может быть пустым");
            if (description.Length < LengthConstants.LENGTH2 || description.Length > LengthConstants.LENGTH200)
                return Result.Failure<TennisCourt>("Описание корта должно быть от 2 до 200 символов");

            return Result.Success();
        }

        private static Result ValidateHourlyRate(decimal hourlyRate)
        {
            if (hourlyRate <= LengthConstants.PRICE100)
                return Result.Failure<TennisCourt>("Стоимость не может быть меньше 100 руб/час"); ;
            if (hourlyRate > LengthConstants.PRICE10_000)
                return Result.Failure<TennisCourt>("Стоимость не может превышать 10000 руб/час");
            return Result.Success();
        }
    }
}
