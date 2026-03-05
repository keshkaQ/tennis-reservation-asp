using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Application.Reservations.Queries;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Contracts.Reservations.Queries;
using TennisReservation.Domain.Enums;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(ILogger<ReservationController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ReservationListItemDto>>> GetAllReservations(
        [FromServices] GetAllReservationsHandler handler,
        CancellationToken cancellationToken)
    {
        var reservations = await handler.HandleAsync(cancellationToken);
        return Ok(reservations);
    }

    [HttpGet("{reservationId:guid}")]
    public async Task<ActionResult<ReservationDto>> GetReservationById(
        [FromRoute] Guid reservationId,
        [FromServices] GetReservationByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetReservationByIdQuery(reservationId), cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        var currentUserId = User.FindFirst("userId")?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && result.Value?.UserId.ToString() != currentUserId)
            return Forbid();

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateReservation(
        [FromBody] CreateReservationCommand request,
        [FromServices] CreateReservationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetReservationById),
            new { reservationId = result.Value.Id },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReservationDto>> UpdateReservation(
        [FromRoute] Guid id,
        [FromBody] UpdateReservationCommand request,
        [FromServices] UpdateReservationHandler handler,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirst("userId")?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && request.UserId.ToString() != currentUserId)
            return Forbid();

        if (id != request.Id)
            return BadRequest(new { error = "ID в маршруте не совпадает с ID бронирования" });

        var result = await handler.HandleAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найдено"))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReservation(
        [FromRoute] Guid id,
        [FromServices] DeleteReservationHandler handler,
        CancellationToken cancellationToken)
    {
        var currentUserId = Guid.TryParse(User.FindFirst("userId")?.Value, out var parsedId)
            ? parsedId : (Guid?)null;
        var isAdmin = User.IsInRole("Admin");

        var result = await handler.HandleAsync(
            new DeleteReservationCommand(id, currentUserId, isAdmin), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найдено"))
                return NotFound(new { error = result.Error });

            if (result.Error.Contains("Нет прав"))
                return Forbid();

            if (result.Error.Contains("прошлое") || result.Error.Contains("нельзя отменить"))
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("Бронирование {ReservationId} успешно удалено", id);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel-reservation")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelReservation(
        [FromRoute] Guid id,
        [FromServices] CancelReservationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new CancelReservationCommand(id), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    [HttpGet("/by-date")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllReservationsByDate(
       [FromQuery] DateOnly date,
       [FromServices] GetAllReservationsByDateHandler handler,
       CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(date, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllReservationByStatus(
   [FromQuery] ReservationStatus status,
   [FromServices] GetAllReservationByStatusHandler handler,
   CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(status, cancellationToken);
        return Ok(result);
    }
}