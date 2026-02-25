using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Domain.Models
{
    public record CredentialsId(Guid Value);

    public class UserCredentials
    {
        private UserCredentials() { }

        private UserCredentials(
            CredentialsId id,
            UserId userId,
            string passwordHash,
            UserRole role)
        {
            Id = id;
            UserId = userId;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }

        public CredentialsId Id { get; }

        [Display(Name = "Идентификатор пользователя")]
        public UserId UserId { get; }

        [Display(Name = "Хэшированный пароль")]
        public string PasswordHash { get; private set; }

        [Display(Name = "Роль пользователя")]
        public UserRole Role { get; private set; }

        [Display(Name = "Дата регистрации")]
        public DateTime CreatedAt { get; }

        [Display(Name = "Дата последнего входа")]
        public DateTime? LastLoginAt { get; private set; }

        [Display(Name = "Количество неверных попыток")]
        public int FailedLoginAttempts { get; private set; }

        [Display(Name = "Блокировка")]
        public DateTime? LockedUntil { get; private set; }

        [ValidateNever]
        public virtual User User { get; private set; } = null!;

        public static Result<UserCredentials> Create(
            UserId userId,
            string password,
            UserRole role = UserRole.User)
        {
            if (userId == null || userId.Value == Guid.Empty)
                return Result.Failure<UserCredentials>("ID пользователя не может быть пустым");

            if (string.IsNullOrWhiteSpace(password))
                return Result.Failure<UserCredentials>("Пароль не может быть пустым");
            if(password.Length < 6)
                return Result.Failure<UserCredentials>("Пароль должен быть не менее 6 символов");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            return new UserCredentials(
                new CredentialsId(Guid.NewGuid()),
                userId,
                passwordHash,
                role);
        }
        public bool VerifyPassword(string password)
        {
            return true;
            //return BCrypt.Net.BCrypt.Verify(password);
        }

        public bool CanLogin => !IsLocked();

        public void ResetLockout() => LockedUntil = null;
        public void LockUntil(DateTime until) => LockedUntil = until;
        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            LockedUntil = null;
        }

        public void RecordFailedAttempt()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= 5)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(15);
            }
        }

        public bool IsLocked() =>
            LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }

        public void ChangePassword(string newPasswordHash, string newSalt)
        {
            PasswordHash = newPasswordHash;
        }
    }
}