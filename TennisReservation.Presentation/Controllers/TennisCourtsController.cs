using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.TennisCourts.Queries;

namespace TennisReservation.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TennisCourtsController : ControllerBase
    {
        private readonly ILogger<TennisCourtsController> _logger;

        public TennisCourtsController(ILogger<TennisCourtsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TennisCourtDto>>> GetAllTennisCourts(
            [FromServices] GetAllTennisCourtsHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var tennisCourts = await handler.HandleAsync(cancellationToken);
                _logger.LogInformation("Получено {Count} кортов", tennisCourts.Value.Count);
                return Ok(tennisCourts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех кортов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{tennisCourtId:guid}")]
        public async Task<ActionResult<TennisCourtDto>> GetTennisCourtById(
            [FromRoute] Guid tennisCourtId,
            [FromServices] GetTennisCourtByIdHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(
                    new GetTennisCourtByIdQuery(tennisCourtId),
                    cancellationToken);

                if (result == null)
                {
                    _logger.LogWarning("Корт {TennisCourtId} не найден", tennisCourtId);
                    return NotFound(new { error = $"Корт с ID {tennisCourtId} не найден" });
                }

                _logger.LogInformation("Корт {TennisCourtId} успешно получен", tennisCourtId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении корта {TennisCourtId}", tennisCourtId);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TennisCourtDto>> CreateTennisCourt(
            [FromBody] CreateTennisCourtCommand request,
            [FromServices] CreateTennisCourtHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Ошибка при создании корта: {Error}", result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Корт создан: ID {CourtId}, Название {Name}",
                    result.Value.Id, request.Name);

                return CreatedAtAction(
                    nameof(GetTennisCourtById),
                    new { tennisCourtId = result.Value.Id },
                    result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании корта {Name}", request.Name);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TennisCourtDto>> UpdateTennisCourt(
            [FromRoute] Guid id,
            [FromBody] UpdateTennisCourtCommand request,
            [FromServices] UpdateTennisCourtHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                if (id != request.Id)
                {
                    _logger.LogWarning("Несовпадающие ID при обновлении корта: маршрут {RouteId} и тело {BodyId}",
                        id, request.Id);
                    return BadRequest(new { error = "ID в маршруте не совпадает с ID корта" });
                }

                var result = await handler.HandleAsync(request, cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найден"))
                    {
                        _logger.LogWarning("Корт {CourtId} не найден при обновлении", id);
                        return NotFound(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при обновлении корта {CourtId}: {Error}", id, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Корт {CourtId} успешно обновлен", id);
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении корта {CourtId}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTennisCourt(
            [FromRoute] Guid id,
            [FromServices] DeleteTennisCourtHandler handler,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await handler.HandleAsync(new DeleteTennisCourtCommand(id), cancellationToken);

                if (result.IsFailure)
                {
                    if (result.Error.Contains("не найден"))
                    {
                        _logger.LogWarning("Корт {CourtId} не найден при удалении", id);
                        return NotFound(new { error = result.Error });
                    }

                    if (result.Error.Contains("активными бронями"))
                    {
                        _logger.LogWarning("Конфликт при удалении корта {CourtId}: {Error}", id, result.Error);
                        return Conflict(new { error = result.Error });
                    }

                    _logger.LogWarning("Ошибка при удалении корта {CourtId}: {Error}", id, result.Error);
                    return BadRequest(new { error = result.Error });
                }

                _logger.LogInformation("Корт {CourtId} успешно удален", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении корта {CourtId}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }
}