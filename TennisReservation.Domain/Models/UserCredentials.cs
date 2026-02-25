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
        public UserId UserId { get; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }
        public virtual User User { get; private set; } = null!;

        public static Result<UserCredentials> Create(
            UserId userId,
            string passwordHash,
            UserRole role = UserRole.User)
        {
            if (userId == null || userId.Value == Guid.Empty)
                return Result.Failure<UserCredentials>("ID пользователя не может быть пустым");
            if (string.IsNullOrWhiteSpace(passwordHash))
                return Result.Failure<UserCredentials>("Хэш пароля не может быть пустым");


            return new UserCredentials(new CredentialsId(Guid.NewGuid()),userId,passwordHash,role);
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

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
        }
    }
}