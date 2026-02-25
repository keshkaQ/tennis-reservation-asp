using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Contracts.Users.Queries;
using TennisReservation.Contracts.Users.Requests;

namespace TennisReservation.API_RP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(
            [FromServices] GetAllUsersHandler handler,
            CancellationToken cancellationToken)
        {
            var users = await handler.HandleAsync(cancellationToken);
            return Ok(users);
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<UserDto>> GetUserById(
            [FromRoute] Guid userId,
            [FromServices] GetUserByIdHandler handler,
            CancellationToken cancellationToken)
        {
            var user = await handler.Handle(new GetUserByIdQuery(userId), cancellationToken);
            if (user == null)  
                return NotFound($"Пользователь с ID {userId} не найден");
            return Ok(user);
        }

        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<UserDto>> GetUserByEmail(
            [FromRoute] string email,
            [FromServices] GetUserByEmailHandler handler,
            CancellationToken cancellationToken)
        {
            var user = await handler.Handle(new GetUserByEmailQuery(email), cancellationToken);
            if (user == null)
                return NotFound($"Пользователь с email {email} не найден");
            return Ok(user);
        }

        [HttpGet("{userId:guid}/with-credentials")]
        public async Task<ActionResult<UserWithCredentialsDto>> GetUserWithCredentials(
            [FromRoute]Guid userId,
            [FromServices] GetUserWithCredentialsHandler handler,
            CancellationToken cancellationToken)
        {
            var user = await handler.Handle(new GetUserWithCredentialsByIdQuery(userId), cancellationToken);
            if (user == null)
                return NotFound($"Пользователь с userId {userId} не найден");
            return Ok(user);
        }


        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(
            [FromBody] CreateUserCommand request,
            [FromServices] CreateUserWithCredentialsHandler handler,
            CancellationToken cancellationToken)
        {
            var user = await handler.HandleAsync(request, cancellationToken);
            if (user.IsFailure)
                return BadRequest("Не удалось создать пользователя");
            return Ok(user.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserDto>> UpdateUser(
            [FromRoute] Guid id,
            [FromBody] UpdateUserRequest request,
            [FromServices] UpdateUserHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new UpdateUserCommand(
                  FirstName: request.FirstName,
                  LastName: request.LastName,
                  Email: request.Email,
                  PhoneNumber: request.PhoneNumber
              );

            var result = await handler.HandleAsync(id,command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] Guid id,
            [FromServices] DeleteUserHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(new DeleteUserByIdQuery(id), cancellationToken);

            if (result.IsFailure)
            {
                return result.Error switch
                {
                    var error when error.Contains("не найден") => NotFound(new { error }),
                    var error when error.Contains("активными бронями") => Conflict(new { error }),
                    _ => BadRequest(new { error = result.Error })
                };
            }

            return NoContent(); 
        }

        ///// <summary>
        ///// Сменить роль пользователя
        ///// </summary>
        //[HttpPatch("{id:guid}/role")]
        //public async Task<IActionResult> ChangeUserRole(
        //    Guid id,
        //    [FromBody] UserRole newRole,
        //    CancellationToken cancellationToken)
        //{
        //    var userResult = await _userRepository.GetByIdWithCredentialAsync(new UserId(id), cancellationToken);

        //    if (userResult.IsFailure)
        //        return NotFound(userResult.Error);

        //    var credentials = userResult.Value.Credentials;
        //    if (credentials == null)
        //        return BadRequest("У пользователя нет учетных данных");

        //    var updateResult = await _credentialsRepository.UpdateRoleAsync(credentials.Id, newRole, cancellationToken);

        //    if (updateResult.IsFailure)
        //        return BadRequest(updateResult.Error);

        //    return NoContent();
        //}

        ///// <summary>
        ///// Заблокировать пользователя
        ///// </summary>
        //[HttpPost("{id:guid}/lock")]
        //public async Task<IActionResult> LockUser(
        //    Guid id,
        //    [FromBody] LockUserRequest request,
        //    CancellationToken cancellationToken)
        //{
        //    var userResult = await _userRepository.GetByIdWithCredentialAsync(new UserId(id), cancellationToken);

        //    if (userResult.IsFailure)
        //        return NotFound(userResult.Error);

        //    var credentials = userResult.Value.Credentials;
        //    if (credentials == null)
        //        return BadRequest("У пользователя нет учетных данных");

        //    var lockUntil = DateTime.UtcNow.AddMinutes(request.LockMinutes);
        //    var result = await _credentialsRepository.LockUntilAsync(credentials.Id, lockUntil, cancellationToken);

        //    if (result.IsFailure)
        //        return BadRequest(result.Error);

        //    return Ok(new { lockedUntil = lockUntil });
        //}

        ///// <summary>
        ///// Разблокировать пользователя
        ///// </summary>
        //[HttpPost("{id:guid}/unlock")]
        //public async Task<IActionResult> UnlockUser(
        //    Guid id,
        //    CancellationToken cancellationToken)
        //{
        //    var userResult = await _userRepository.GetByIdWithCredentialAsync(new UserId(id), cancellationToken);

        //    if (userResult.IsFailure)
        //        return NotFound(userResult.Error);

        //    var credentials = userResult.Value.Credentials;
        //    if (credentials == null)
        //        return BadRequest("У пользователя нет учетных данных");

        //    var result = await _credentialsRepository.ResetLockoutAsync(credentials.Id, cancellationToken);

        //    if (result.IsFailure)
        //        return BadRequest(result.Error);

        //    return Ok();
        //}

        ///// <summary>
        ///// Получить статистику пользователя
        ///// </summary>
        //[HttpGet("{id:guid}/stats")]
        //public async Task<ActionResult<UserStatsDto>> GetUserStats(
        //    Guid id,
        //    CancellationToken cancellationToken)
        //{
        //    var userResult = await _userRepository.GetByIdWithCredentialAsync(new UserId(id), cancellationToken);

        //    if (userResult.IsFailure)
        //        return NotFound(userResult.Error);

        //    var user = userResult.Value;
        //    var stats = new UserStatsDto
        //    {
        //        UserId = user.Id.Value,
        //        Email = user.Email,
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        PhoneNumber = user.PhoneNumber,
        //        RegistrationDate = user.RegistrationDate,
        //        ReservationsCount = user.Reservations.Count,
        //        IsActive = user.Credentials?.CanLogin ?? false,
        //        LastLoginAt = user.Credentials?.LastLoginAt,
        //        FailedLoginAttempts = user.Credentials?.FailedLoginAttempts ?? 0,
        //        IsLocked = user.Credentials?.IsLocked() ?? false,
        //        LockedUntil = user.Credentials?.LockedUntil,
        //        Role = user.Credentials?.Role ?? UserRole.User
        //    };

        //    return Ok(stats);
        //}
    }
   
}