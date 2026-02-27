using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Contracts.TennisCourts.Commands
{
    public record CreateTennisCourtCommand
    (
        [property: Required(ErrorMessage = "Название корта обязательно к заполнению")]
        [property: MinLength(3, ErrorMessage = "Название корта должно содержать минимум 3 символа")]
        [property: MaxLength(100, ErrorMessage = "Название корта не может превышать 100 символов")]
        [property: RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s]+$",
            ErrorMessage = "Название корта может содержать только буквы, цифры и пробелы")]
        [property: Display(Name = "Название корта")]
        string Name,

        [property: Required(ErrorMessage = "Часовая ставка обязательна к заполнению")]
        [property: Range(100, 10000, ErrorMessage = "Часовая ставка должна быть от 100 до 10000")]
        [property: DataType(DataType.Currency)]
        [property: Display(Name = "Часовая ставка (₽/час)")]
        decimal HourlyRate,

        [property: MaxLength(500, ErrorMessage = "Описание не может превышать 500 символов")]
        [property: MinLength(3, ErrorMessage = "Описание корта должно содержать минимум 3 символа")]
        [property: Display(Name = "Описание корта")]
        string Description
    );
}