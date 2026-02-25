using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Domain.Enums;
public record UserWithCredentialsDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime RegistrationDate,
    int ReservationsCount,
    UserRole Role,
    DateTime? LastLoginAt,
    int FailedLoginAttempts,
    bool IsLocked,
    DateTime? LockedUntil
) : UserDto(
    UserId,
    FirstName,
    LastName,
    Email,
    PhoneNumber,
    RegistrationDate,
    ReservationsCount
);