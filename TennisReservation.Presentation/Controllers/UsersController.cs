using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.Auth;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Dto.TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Queries;
using TennisReservation.Domain.Enums;

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
        var result = await handler.HandleAsync(cancellationToken);

        if (result.IsFailure)
            return StatusCode(500, new { error = result.Error });

        return Ok(result.Value);
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

        var result = await handler.HandleAsync(new GetUserByIdQuery(userId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("by-email/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserByEmail(
        [FromRoute] string email,
        [FromServices] GetUserByEmailHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetUserByEmailQuery(email), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{userId:guid}/with-credentials")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserWithCredentials(
        [FromRoute] Guid userId,
        [FromServices] GetUserWithCredentialsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetUserWithCredentialsByIdQuery(userId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand request,
        [FromServices] CreateUserWithCredentialsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetUserById), new { userId = result.Value.UserId }, result.Value);
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

        if (id != request.Id)
            return BadRequest(new { error = "ID в маршруте не совпадает с ID в теле запроса" });

        var result = await handler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найден"))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(
        [FromRoute] Guid id,
        [FromServices] DeleteUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteUserByIdCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найден"))
                return NotFound(new { error = result.Error });

            if (result.Error.Contains("активными бронями") || result.Error.Contains("в использовании"))
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("Пользователь {UserId} успешно удален", id);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserQuery request,
        [FromServices] UserService userService,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Password,
            UserRole.User);

        var result = await userService.Register(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("уже существует") || result.Error.Contains("занят"))
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetUserById), new { userId = result.Value.UserId }, result.Value);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        [FromServices] UserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.Login(request.Email, request.Password);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        HttpContext.Response.Cookies.Append("jwt-cookies", result.Value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(12)
        });

        return Ok();
    }

    [HttpPost("{id}/change-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(
        [FromRoute] Guid id,
        [FromBody] ChangeRoleRequest request,
        [FromServices] ChangeRoleHandler handler,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst("userId")?.Value;
        if (currentUserId != null && id == Guid.Parse(currentUserId))
            return BadRequest(new { error = "Нельзя изменить роль самому себе" });

        var result = await handler.HandleAsync(new ChangeRoleCommand(id, request.Role), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найден"))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok();
    }

    [HttpPost("{id}/change-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangePassword(
        [FromRoute] Guid id,
        [FromBody] ChangePasswordRequest request,
        [FromServices] ChangePasswordHandler handler,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst("userId")?.Value;
        if (currentUserId != null && id == Guid.Parse(currentUserId))
            return BadRequest(new { error = "Нельзя изменить пароль самому себе" });

        var result = await handler.HandleAsync(new ChangePasswordCommand(id, request.NewPassword), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найден"))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok();
    }

    [HttpPost("{id}/lock-user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LockUser(
        [FromRoute] Guid id,
        [FromBody] LockUserRequest request,
        [FromServices] LockUserHandler handler,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst("userId")?.Value;
        if (currentUserId != null && id == Guid.Parse(currentUserId))
            return BadRequest(new { error = "Нельзя заблокировать самого себя" });

        if (request.LockTime <= DateTime.UtcNow)
            return BadRequest(new { error = "Дата блокировки должна быть в будущем" });

        var result = await handler.HandleAsync(new LockUserCommand(id, request.LockTime), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найден"))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("Пользователь {UserId} заблокирован до {Date}", id, request.LockTime);
        return Ok();
    }

    [HttpPost("{id}/unlock-user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnlockUser(
        [FromRoute] Guid id,
        [FromServices] UnlockUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new UnlockUserCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найден"))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok();
    }
}