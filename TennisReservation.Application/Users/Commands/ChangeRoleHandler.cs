using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users.Commands
{
    public class ChangeRoleHandler
    {
        private readonly IUserCredentialsRepository _userCredentialsRepository;
        private readonly ILogger<ChangeRoleHandler> _logger;

        public ChangeRoleHandler(IUserCredentialsRepository userCredentialsRepository, ILogger<ChangeRoleHandler> logger)
        {
            _userCredentialsRepository = userCredentialsRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(ChangeRoleCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var credentials = await _userCredentialsRepository.GetWithUserByIdAsync(command.UserId);
                if (credentials.IsFailure || credentials.Value == null)
                    return Result.Failure("Пользователь не найден");

                if (credentials.Value.Role == command.Role)
                    return Result.Failure("Нельзя изменить роль на такую же");

                credentials.Value.ChangeRole(command.Role);
                var result = await _userCredentialsRepository.UpdateAsync(credentials.Value);
                if (result.IsFailure)
                    return Result.Failure("Не удалось сменить роль");

                _logger.LogInformation("Роль пользователя {UserId} изменена на {Role}", command.UserId, command.Role);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при смене роли пользователя {UserId}", command.UserId);
                return Result.Failure("Не удалось сменить роль");
            }
        }
    }
}
