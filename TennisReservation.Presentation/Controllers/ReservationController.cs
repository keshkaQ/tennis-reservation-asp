using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Application.Reservations.Queries;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Contracts.Reservations.Queries;

namespace TennisReservation.Presentation.Controllers
{
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
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations(
            [FromServices] GetAllReservationsHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var reservations = await handler.HandleAsync(cancellationToken);
                _logger.LogInformation("Получено {Count} бронирований", reservations.Count());
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех бронирований");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{reservationId:guid}")]
        public async Task<ActionResult<ReservationDto>> GetReservationById(
            [FromRoute] Guid reservationId,
            [FromServices] GetReservationByIdHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(
                    new GetReservationByIdQuery(reservationId),
                    cancellationToken);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Бронирование {ReservationId} не найдено", reservationId);
                    return NotFound(new { error = $"Бронирование с ID {reservationId} не найдено" });
                }

                // Обычный пользователь может смотреть только свою бронь
                var currentUserId = User.FindFirst("userId")?.Value;
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && result.Value.UserId.ToString() != currentUserId)
                    return Forbid();

                _logger.LogInformation("Бронирование {ReservationId} успешно получено", reservationId);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении бронирования {ReservationId}", reservationId);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(
            [FromBody] CreateReservationCommand request,
            [FromServices] CreateReservationHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Ошибка при создании бронирования: {Error}", result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Бронирование создано: ID {ReservationId}, Корт {CourtId}, Пользователь {UserId}",
                    result.Value.Id, request.TennisCourtId, request.UserId);

                return CreatedAtAction(
                    nameof(GetReservationById),
                    new { reservationId = result.Value.Id },
                    result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании бронирования для корта {CourtId}, пользователя {UserId}",
                    request.TennisCourtId, request.UserId);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
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

            try
            {
                if (id != request.Id)
                {
                    _logger.LogWarning("Несовпадающие ID при обновлении: маршрут {RouteId} и тело {BodyId}",
                        id, request.Id);
                    return BadRequest(new { error = "ID в маршруте не совпадает с ID бронирования" });
                }

                var result = await handler.HandleAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найдено"))
                    {
                        _logger.LogWarning("Бронирование {ReservationId} не найдено при обновлении", id);
                        return NotFound(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при обновлении бронирования {ReservationId}: {Error}", id, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Бронирование {ReservationId} успешно обновлено", id);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении бронирования {ReservationId}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteReservation(
            [FromRoute] Guid id,
            [FromServices] DeleteReservationHandler handler,
            CancellationToken cancellationToken)
        {
            var currentUserId = Guid.TryParse(User.FindFirst("userId")?.Value, out var parsedId)
                ? parsedId
                : (Guid?)null;
            var isAdmin = User.IsInRole("Admin");

            var result = await handler.HandleAsync(
                new DeleteReservationCommand(id, currentUserId, isAdmin),
                cancellationToken);

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
    }
}