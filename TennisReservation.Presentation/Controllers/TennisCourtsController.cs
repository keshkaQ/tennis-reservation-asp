using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.TennisCourts.Commands.CreateTennisCourt;
using TennisReservation.Application.TennisCourts.Commands.DeleteTennisCourt;
using TennisReservation.Application.TennisCourts.Commands.UpdateTennisCourt;
using TennisReservation.Application.TennisCourts.Queries.GetAllReservationsByCourtId;
using TennisReservation.Application.TennisCourts.Queries.GetCourtAvailability;
using TennisReservation.Application.TennisCourts.Queries.GetTennisCourtById;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.TennisCourts.Requests;

[Route("api/[controller]")]
[ApiController]
public class TennisCourtsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TennisCourtDto>>> GetAllTennisCourts(
        [FromServices] GetAllTennisCourtsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);

        if (result.IsFailure)
            return StatusCode(500, new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{tennisCourtId:guid}")]
    public async Task<ActionResult<TennisCourtDto>> GetTennisCourtById(
        [FromRoute] Guid tennisCourtId,
        [FromServices] GetTennisCourtByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetTennisCourtByIdQuery(tennisCourtId), cancellationToken);

        if (result == null)
            return NotFound(new { error = $"Корт с ID {tennisCourtId} не найден" });

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TennisCourtDto>> CreateTennisCourt(
        [FromBody] CreateTennisCourtRequest request,
        [FromServices] CreateTennisCourtHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateTennisCourtCommand(request.Name, request.HourlyRate, request.Description);
        var result = await handler.HandleAsync(command, cancellationToken); 
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetTennisCourtById),
            new { tennisCourtId = result.Value.Id },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TennisCourtDto>> UpdateTennisCourt(
        [FromRoute] Guid id,
        [FromBody] UpdateTennisCourtRequest request,
        [FromServices] UpdateTennisCourtHandler handler,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(new { error = "ID в маршруте не совпадает с ID корта" });

        var command = new UpdateTennisCourtCommand(id, request.Name, request.HourlyRate, request.Description);
        var result = await handler.HandleAsync(command, cancellationToken);
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
    public async Task<IActionResult> DeleteTennisCourt(
        [FromRoute] Guid id,
        [FromServices] DeleteTennisCourtHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteTennisCourtCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("не найден"))
                return NotFound(new { error = result.Error });

            if (result.Error.Contains("активными бронями"))
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpGet("availability/{courtId:guid}")]
    public async Task<IActionResult> GetAvailability(
    [FromRoute] Guid courtId,
    [FromQuery] DateTime startTime,
    [FromQuery] DateTime endTime,
    [FromServices] GetCourtAvailabilityHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(courtId, startTime, endTime, cancellationToken);
        if(result.IsFailure)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("{courtId:guid}/reservations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllCourtReservations(
    [FromRoute] Guid courtId,
    [FromServices] GetAllReservationsByCourtIdHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(courtId, cancellationToken);
        return Ok(result);
    }
}