using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;

namespace TennisReservation.Presentation.Pages.Users
{
    public class MyReservationsModel : PageModel
    {
        private readonly GetAllReservationsByUserIdHandler _getMyReservationsHandler;
        private readonly CancelReservationHandler _cancelReservationHandler;

        public MyReservationsModel(
            GetAllReservationsByUserIdHandler getMyReservationsHandler,
            CancelReservationHandler cancelReservationHandler)
        {
            _getMyReservationsHandler = getMyReservationsHandler;
            _cancelReservationHandler = cancelReservationHandler;
        }

        public IEnumerable<ReservationListItemDto> Reservations { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            Reservations = await _getMyReservationsHandler.HandleAsync(userId, CancellationToken.None);
            return Page();
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