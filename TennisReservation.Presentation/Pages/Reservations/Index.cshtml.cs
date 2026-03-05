using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;

namespace TennisReservation.Presentation.Pages.Reservations
{
    public class IndexModel : PageModel
    {
        private readonly GetAllReservationsHandler _getAllReservationsHandler;
        private readonly DeleteReservationHandler _deleteReservationHandler;
        private readonly CancelReservationHandler _cancelReservationHandler;

        public IndexModel(GetAllReservationsHandler getAllReservationsHandler,
             CancelReservationHandler cancelReservationHandler,
             DeleteReservationHandler deleteReservationHandler)
        {
            _getAllReservationsHandler = getAllReservationsHandler;
            _cancelReservationHandler = cancelReservationHandler;
            _deleteReservationHandler = deleteReservationHandler;
        }

        public IEnumerable<ReservationListItemDto> Reservations { get; set; } = [];
        public async Task<IActionResult> OnGetAsync()
        {
            Reservations = await _getAllReservationsHandler.HandleAsync(CancellationToken.None);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var currentUserId = Guid.TryParse(User.FindFirst("userId")?.Value, out var parsedId)
                ? parsedId: (Guid?)null;
            var isAdmin = User.IsInRole("Admin");
            var command = new DeleteReservationCommand(id, currentUserId, isAdmin);
            var reservationResult = await _deleteReservationHandler.HandleAsync(command, CancellationToken.None);
            if(reservationResult.IsFailure)
            {
                if (reservationResult.Error.Contains("не найдено"))
                {
                    TempData["ErrorMessage"] = "Бронирование не найдено";
                    return NotFound(new { error = reservationResult.Error });
                }
                if (reservationResult.Error.Contains("Нет прав"))
                    return Forbid();
                return BadRequest(new { error = reservationResult.Error });
            }

            TempData["SuccessMessage"] = $"Бронирование успешно удалено";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            var command = new CancelReservationCommand(id);
            var result = await _cancelReservationHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsFailure)
                TempData["ErrorMessage"] = result.Error;
            else
                TempData["SuccessMessage"] = "Бронирование успешно отменено";

            return RedirectToPage();
        }
    }
}