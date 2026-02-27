using System.ComponentModel.DataAnnotations;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Users.Commands
{
    public record CreateUserCommand
    (
        [property: Required(ErrorMessage = "Имя обязательно к заполнению")]
        [property: MinLength(2, ErrorMessage = "Имя должно содержать минимум 2 символа")]
        [property: MaxLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
        [property: RegularExpression(@"^[a-zA-Zа-яА-Я\s]+$",
            ErrorMessage = "Имя может содержать только буквы и пробелы")]
        [property: Display(Name = "Имя")]
        string FirstName,

        [property: Required(ErrorMessage = "Фамилия обязательна к заполнению")]
        [property: MinLength(2, ErrorMessage = "Фамилия должна содержать минимум 2 символа")]
        [property: MaxLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
        [property: RegularExpression(@"^[a-zA-Zа-яА-Я\s]+$",
            ErrorMessage = "Фамилия может содержать только буквы и пробелы")]
        [property: Display(Name = "Фамилия")]
        string LastName,

        [property: Required(ErrorMessage = "Электронный адрес обязателен к заполнению")]
        [property: EmailAddress(ErrorMessage = "Введите корректный электронный адрес")]
        [property: MaxLength(255, ErrorMessage = "Электронный адрес не может превышать 255 символов")]
        [property: RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Введите корректный электронный адрес (например: name@domain.com)")]
        [property: Display(Name = "Электронный адрес")]
        string Email,

        [property: Required(ErrorMessage = "Номер телефона обязателен к заполнению")]
        [property: Phone(ErrorMessage = "Введите корректный номер телефона")]
        [property: MaxLength(20, ErrorMessage = "Номер телефона не может превышать 20 символов")]
        [property: RegularExpression(@"^(\+7|8)?[\s\-]?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$",
            ErrorMessage = "Введите корректный номер телефона (например: +7 (999) 123-45-67 или 89991234567)")]
        [property: Display(Name = "Номер телефона")]
        string PhoneNumber,

        [property: Required(ErrorMessage = "Пароль обязателен к заполнению")]
        [property: MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
        [property: MaxLength(100, ErrorMessage = "Пароль не может превышать 100 символов")]
        [property: RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Пароль должен содержать как минимум одну заглавную букву, одну строчную букву, одну цифру и один специальный символ")]
        [property: DataType(DataType.Password)]
        [property: Display(Name = "Пароль")]
        string Password,

        UserRole Role = UserRole.User
    );
}