using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TennisReservation.Domain.Constants;

namespace TennisReservation.Domain.Models
{
    public record UserId(Guid Value);

    public class User
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        private User() { }

        private User(UserId id, string firstName, string lastName, string email, string phoneNumber)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            RegistrationDate = DateTime.UtcNow;
            _reservations = new List<Reservation>();
        }

        public UserId Id { get; }

        [Required(ErrorMessage = "Имя обязательно к заполнению")]
        [MaxLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
        [Display(Name = "Имя")]
        public string FirstName { get; private set; }

        [Required(ErrorMessage = "Фамилия обязательна к заполнению")]
        [MaxLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
        [Display(Name = "Фамилия")]
        public string LastName { get; private set; }

        [Required(ErrorMessage = "Почта обязательна к заполнению")]
        [EmailAddress(ErrorMessage = "Введите корректный адрес")]
        [MaxLength(255, ErrorMessage = "Почта не может превышать 255 символов")]
        [Display(Name = "Почта")]
        public string Email { get; private set; }

        [Required(ErrorMessage = "Номер телефона обязателен к заполнению")]
        [Phone(ErrorMessage = "Введите корректный номер телефона")]
        [MaxLength(20, ErrorMessage = "Номер телефона не может превышать 20 символов")]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; private set; }
        public DateTime RegistrationDate { get; }
        private List<Reservation> _reservations;
        public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

        public UserCredentials? Credentials { get; private set; }
        public void SetCredentials(UserCredentials credentials) => Credentials = credentials;

        public static Result<User> Create(string firstName, string lastName, string email, string phoneNumber)
        {
            firstName = firstName?.Trim() ?? "";
            lastName = lastName?.Trim() ?? "";
            email = email?.Trim().ToLower() ?? "";
            phoneNumber = phoneNumber?.Trim() ?? "";

            // Используем общие методы валидации
            var validationResult = ValidateNames(firstName, lastName)
                .Bind(() => ValidateEmail(email))
                .Bind(() => ValidatePhoneNumber(phoneNumber));

            if (validationResult.IsFailure)
                return Result.Failure<User>(validationResult.Error);

            return new User(
                new UserId(Guid.NewGuid()),
                firstName,
                lastName,
                email,
                phoneNumber);
        }

        public Result Update(string firstName, string lastName, string email, string phoneNumber)
        {
            firstName = firstName?.Trim() ?? "";
            lastName = lastName?.Trim() ?? "";
            email = email?.Trim().ToLower() ?? "";
            phoneNumber = phoneNumber?.Trim() ?? "";

            var validationResult = ValidateNames(firstName, lastName)
                .Bind(() => ValidateEmail(email))
                .Bind(() => ValidatePhoneNumber(phoneNumber));

            if (validationResult.IsFailure)
                return validationResult;

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;

            return Result.Success();
        }

        private static Result ValidateNames(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                return Result.Failure("Имя пользователя не может быть пустым");
            if (firstName.Length < LengthConstants.LENGTH2 || firstName.Length > LengthConstants.LENGTH50)
                return Result.Failure("Имя пользователя должно быть от 2 до 50 символов");
            if (firstName.Any(char.IsDigit))
                return Result.Failure("Имя пользователя не должно содержать цифр");

            if (string.IsNullOrWhiteSpace(lastName))
                return Result.Failure("Фамилия пользователя не может быть пустой");
            if (lastName.Length < LengthConstants.LENGTH2 || lastName.Length > LengthConstants.LENGTH50)
                return Result.Failure("Фамилия пользователя должна быть от 2 до 50 символов");
            if (lastName.Any(char.IsDigit))
                return Result.Failure("Фамилия пользователя не должна содержать цифр");

            return Result.Success();
        }

        private static Result ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure("Email не может быть пустым");
            if (email.Length < LengthConstants.LENGTH5 || email.Length > LengthConstants.LENGTH255)
                return Result.Failure("Email должен содержать от 5 до 255 символов");
            if (!EmailRegex.IsMatch(email))
                return Result.Failure("Некорректный формат email");

            return Result.Success();
        }

        private static Result ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return Result.Failure("Номер телефона не может быть пустым");

            var cleanedPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (cleanedPhone.Length < 10 || cleanedPhone.Length > 15)
                return Result.Failure("Номер телефона должен содержать от 10 до 15 цифр");

            return Result.Success();
        }
    }
}