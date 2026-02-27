using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Contracts.TennisCourts.Commands
{
    public record UpdateTennisCourtCommand
    (
        Guid Id,

    [Required(ErrorMessage = "Название корта обязательно к заполнению")]
    [MinLength(3, ErrorMessage = "Название корта должно содержать минимум 3 символа")]
    [MaxLength(100, ErrorMessage = "Название корта не может превышать 100 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s]+$",
        ErrorMessage = "Название корта может содержать только буквы, цифры и пробелы")]
    [Display(Name = "Название корта")]
    string Name,

    [Required(ErrorMessage = "Часовая ставка обязательна к заполнению")]
    [Range(100, 10000, ErrorMessage = "Часовая ставка должна быть от 100 до 10000")]
    [DataType(DataType.Currency)]
    [Display(Name = "Часовая ставка (₽/час)")]
    decimal HourlyRate,

    [MaxLength(500, ErrorMessage = "Описание не может превышать 500 символов")]
    [MinLength(3, ErrorMessage = "Описание корта должно содержать минимум 3 символа")]
    [Display(Name = "Описание корта")]
    string Description
    );
}
