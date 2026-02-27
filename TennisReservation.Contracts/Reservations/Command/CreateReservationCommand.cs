using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Contracts.Reservations.Command
{
    public record CreateReservationCommand
    (
        [property: Required(ErrorMessage = "Выберите теннисный корт")]
        [property: Display(Name = "Теннисный корт")]
        Guid TennisCourtId,

        [property: Required(ErrorMessage = "Выберите клиента")]
        [property: Display(Name = "Клиент")]
        Guid UserId,

        [property: Required(ErrorMessage = "Укажите дату и время начала")]
        [property: Display(Name = "Начало")]
        [property: DataType(DataType.DateTime)]
        DateTime StartTime,

        [property: Required(ErrorMessage = "Укажите дату и время окончания")]
        [property: Display(Name = "Окончание")]
        [property: DataType(DataType.DateTime)]
        DateTime EndTime
    );
}