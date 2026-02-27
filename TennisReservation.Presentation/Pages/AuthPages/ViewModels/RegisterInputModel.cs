using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Presentation.Pages.AuthPages.ViewModels
{
    public class RegisterInputModel
    {
        [Required(ErrorMessage = "Имя обязательно к заполнению")]
        [MinLength(2, ErrorMessage = "Имя должно содержать минимум 2 символа")]
        [MaxLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я\s]+$",
            ErrorMessage = "Имя может содержать только буквы")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна к заполнению")]
        [MinLength(2, ErrorMessage = "Фамилия должна содержать минимум 2 символа")]
        [MaxLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я\s]+$",
            ErrorMessage = "Фамилия может содержать только буквы")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Электронный адрес обязателен к заполнению")]
        [EmailAddress(ErrorMessage = "Введите корректный электронный адрес")]
        [MaxLength(255, ErrorMessage = "Электронный адрес не может превышать 255 символов")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Введите корректный электронный адрес (например: name@domain.com)")]
        [Display(Name = "Электронный адрес")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Номер телефона обязателен к заполнению")]
        [Phone(ErrorMessage = "Введите корректный номер телефона")]
        [MaxLength(20, ErrorMessage = "Номер телефона не может превышать 20 символов")]
        [RegularExpression(@"^(\+7|8)?[\s\-]?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$",
            ErrorMessage = "Введите корректный номер телефона (например: +7 (999) 123-45-67 или 89991234567)")]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть от 8 до 100 символов")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Пароль должен содержать как минимум одну заглавную букву, одну строчную букву, одну цифру и один специальный символ")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}