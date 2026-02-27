using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.Auth;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Dto.TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Queries;

namespace TennisReservation.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
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
            var currentUserId = User.FindFirst("userId")?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != userId.ToString())
                return Forbid();

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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
                _logger.LogError(ex, "Критическая ошибка в GetUserWithCredentials({UserId})", userId);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
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

                return CreatedAtAction(nameof(GetUserById), new { userId = result.Value.UserId }, result.Value);
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
            var currentUserId = User.FindFirst("userId")?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != id.ToString())
                return Forbid();

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
        [Authorize(Roles = "Admin")]
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserQuery request,
            [FromServices] UserService userService,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new CreateUserCommand(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PhoneNumber,
                    request.Password,
                    Domain.Enums.UserRole.User
                );

                var result = await userService.Register(command, cancellationToken);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Ошибка при регистрации пользователя {Email}: {Error}",
                        request.Email, result.Error);

                    if (result.Error.Contains("уже существует") || result.Error.Contains("занят"))
                        return Conflict(new { error = result.Error });

                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Пользователь {Email} успешно зарегистрирован, ID: {UserId}",
                    request.Email, result.Value.UserId);

                return CreatedAtAction(nameof(GetUserById), new { userId = result.Value.UserId }, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при регистрации пользователя {Email}", request.Email);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginUserRequest request,
            [FromServices] UserService userService,
            CancellationToken cancellationToken)
        {
            var token = await userService.Login(request.Email, request.Password);
            if (token == null)
                return Unauthorized(new { error = "Неверный email или пароль" });

            HttpContext.Response.Cookies.Append("jwt-cookies", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(12)
            });
            return Ok();
        }
    }
}