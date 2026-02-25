using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.TennisCourts.Queries;

namespace TennisReservation.API_RP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TennisCourtsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TennisCourtDto>>> GetAllTennisCourts(
            [FromServices] GetAllTennisCourtsHandler handler, CancellationToken cancellationToken)
        {
            var tennisCourts = await handler.HandleAsync(cancellationToken);
            return Ok(tennisCourts);
        }

        [HttpGet("{tennisCourtId:guid}")]
        public async Task<ActionResult<TennisCourtDto>> GetTennisCourtById(
        [FromRoute] Guid tennisCourtId,
        [FromServices] GetTennisCourtByIdHandler handler,
        CancellationToken cancellationToken)
        {
            var tennisCourt = await handler.HandleAsync(new GetTennisCourtByIdQuery(tennisCourtId), cancellationToken);
            if (tennisCourt == null)
                return NotFound($"Корт с ID {tennisCourtId} не найден");
            return Ok(tennisCourt);
        }

        [HttpPost]
        public async Task<ActionResult<TennisCourtDto>> CreateTennisCourt(
           [FromBody] CreateTennisCourtCommand request,
           [FromServices] CreateTennisCourtHandler handler,
           CancellationToken cancellationToken)
        {
            var tennisCourt = await handler.HandleAsync(request, cancellationToken);
            if (tennisCourt.IsFailure)
                return BadRequest("Не удалось создать корт");
            return Ok(tennisCourt.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TennisCourtDto>> UpdateTennisCourt(
            [FromRoute] Guid id,
            [FromBody] UpdateTennisCourtCommand request,
            [FromServices] UpdateTennisCourtHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(id,request, cancellationToken);
            if(result.IsFailure)
                return BadRequest(new { error = result.Error });
            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTennisCourt(
           [FromRoute] Guid id,
           [FromServices] DeleteTennisCourtHandler handler,
           CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(new DeleteTennisCourtByIdQuery(id), cancellationToken);

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
    }
}