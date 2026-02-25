using Microsoft.AspNetCore.Mvc;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Application.Reservations.Queries;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Contracts.Reservations.Queries;

namespace TennisReservation.API_RP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations(
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
            var reservation = await handler.HandleAsync(new GetReservationByIdQuery(reservationId), cancellationToken);
            if (reservation.IsFailure)
                return NotFound($"Бронирование с ID {reservationId} не найдено");
            return Ok(reservation);
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(
         [FromBody] CreateReservationCommand request,
         [FromServices] CreateReservationHandler handler,
         CancellationToken cancellationToken)
        {
            var reservation = await handler.HandleAsync(request, cancellationToken);
            if(reservation.IsFailure)
                return BadRequest("Не удалось создать бронирование");
            return Ok(reservation.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ReservationDto>> UpdateReservation(
            [FromRoute] Guid id,
            [FromBody] UpdateReservationCommand request,
            [FromServices] UpdateReservationHandler handler,
            CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest(new { error = "ID в маршруте не совпадает с ID бронирования" });
            }

            var result = await handler.HandleAsync(request, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTennisCourt(
            [FromRoute] Guid id,
            [FromServices] DeleteReservationHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(new DeleteReservationByIdQuery(id), cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });
            return NoContent();
        }
    }
}
