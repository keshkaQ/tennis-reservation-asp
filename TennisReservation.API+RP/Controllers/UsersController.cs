using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Queries;

namespace TennisReservation.API_RP.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromServices] GetAllUsersHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(cancellationToken);
                if (result.IsFailure)
                {
                    _logger.LogWarning("Ошибка при получении всех пользователей: {Error}", result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Получено {Count} пользователей", result.Value.Count);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в GetAllUsers: {Message}", ex.Message);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserById(
            [FromRoute] Guid userId,
            [FromServices] GetUserByIdHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(new GetUserByIdQuery(userId), cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найден"))
                    {
                        _logger.LogWarning("Пользователь {UserId} не найден", userId);
                        return NotFound(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при получении пользователя {UserId}: {Error}", userId, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Пользователь {UserId} успешно получен", userId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в GetUserById({UserId})", userId);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetUserByEmail(
            [FromRoute] string email,
            [FromServices] GetUserByEmailHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(new GetUserByEmailQuery(email), cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найден"))
                    {
                        _logger.LogWarning("Пользователь с email {Email} не найден", email);
                        return NotFound(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при получении пользователя по email {Email}: {Error}", email, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Пользователь с email {Email} успешно получен", email);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в GetUserByEmail({Email})", email);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{userId:guid}/with-credentials")]
        public async Task<IActionResult> GetUserWithCredentials(
            [FromRoute] Guid userId,
            [FromServices] GetUserWithCredentialsHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(new GetUserWithCredentialsByIdQuery(userId), cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найден"))
                    {
                        _logger.LogWarning("Пользователь с учетными данными {UserId} не найден", userId);
                        return NotFound(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при получении пользователя с учетными данными {UserId}: {Error}", userId, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Пользователь с учетными данными {UserId} успешно получен", userId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Критическая ошибка в GetUserWithCredentials({UserId})", userId);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserCommand request,
            [FromServices] CreateUserWithCredentialsHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Ошибка при создании пользователя {Email}: {Error}", request.Email, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Пользователь с email {Email} успешно создан, ID: {UserId}",
                    request.Email, result.Value.UserId);

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { userId = result.Value.UserId },
                    result.Value
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в CreateUser для email {Email}", request.Email);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] Guid id,
            [FromBody] UpdateUserCommand request,
            [FromServices] UpdateUserHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                if (id != request.Id)
                {
                    _logger.LogWarning("Несовпадающие ID: маршрут {RouteId} vs тело {BodyId}", id, request.Id);
                    return BadRequest(new { error = "ID в маршруте не совпадает с ID в теле запроса" });
                }

                var result = await handler.HandleAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найден"))
                    {
                        _logger.LogWarning("Пользователь {UserId} не найден при обновлении", id);
                        return NotFound(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при обновлении пользователя {UserId}: {Error}", id, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Пользователь {UserId} успешно обновлен", id);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в UpdateUser({UserId})", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] Guid id,
            [FromServices] DeleteUserHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(new DeleteUserByIdCommand(id), cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найден"))
                    {
                        _logger.LogWarning("Пользователь {UserId} не найден при удалении", id);
                        return NotFound(new { error = result.Error });
                    }

                    if (result.Error.Contains("активными бронями") || result.Error.Contains("в использовании"))
                    {
                        _logger.LogWarning("Конфликт при удалении пользователя {UserId}: {Error}", id, result.Error);
                        return Conflict(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при удалении пользователя {UserId}: {Error}", id, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Пользователь {UserId} успешно удален", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в DeleteUser({UserId})", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }
}