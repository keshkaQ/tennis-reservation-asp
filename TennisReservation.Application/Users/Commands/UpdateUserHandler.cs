using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users.Commands
{
    public class UpdateUserHandler
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<UpdateUserHandler> _logger;
        public UpdateUserHandler(IUsersRepository usersRepository, ILogger<UpdateUserHandler> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }

        public async Task<Result<UserDto>> HandleAsync(UpdateUserCommand command,CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _usersRepository.GetByIdAsync(new UserId(command.Id), cancellationToken);
              
                if (existingUser.IsFailure)
                {
                    _logger.LogWarning("Пользователь с ID {UserId} не найден", command.Id);
                    return Result.Failure<UserDto>("Пользователь не найден");
                }
                var userToUpdate = existingUser.Value;

                var updateResult = userToUpdate.Update(
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.PhoneNumber
                );

                if (updateResult.IsFailure)
                {
                    _logger.LogWarning("Ошибка валидации при обновлении пользователя {UserId}: {Error}",command.Id,updateResult.Error);
                    return Result.Failure<UserDto>(updateResult.Error);
                }
                var saveResult = await _usersRepository.UpdateAsync(
                   userToUpdate,
                   cancellationToken);

                if (saveResult.IsFailure)
                {
                    _logger.LogError("Не удалось сохранить пользователя {UserId} в БД",command.Id);
                    return Result.Failure<UserDto>(saveResult.Error);
                }

                var dto = new UserDto(
                    userToUpdate.Id.Value,
                    userToUpdate.FirstName,
                    userToUpdate.LastName,
                    userToUpdate.Email,
                    userToUpdate.PhoneNumber,
                    userToUpdate.RegistrationDate,
                    userToUpdate.Reservations?.Count ?? 0
                );

                _logger.LogInformation("Пользователь {UserId} успешно обновлен",userToUpdate.Id.Value);

                return Result.Success(dto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при обновлении пользователя {UserId}",command.Id);
                return Result.Failure<UserDto>("Не удалось обновить пользователя");
            }
        }

    }
}
