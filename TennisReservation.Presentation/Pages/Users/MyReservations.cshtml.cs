using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Reservations.Commands.CancelReservation;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Enums;

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
        public string SortField { get; set; } = "StartTime";
        public bool SortAsc { get; set; } = true;
        public string StatusFilter { get; set; } = "All";

        public async Task<IActionResult> OnGetAsync(
            string sortField = "StartTime",
            bool sortAsc = true,
            string statusFilter = "All")
        {
            SortField = sortField;
            SortAsc = sortAsc;
            StatusFilter = statusFilter;

            var userId = Guid.Parse(User.FindFirst("userId")!.Value);
            var all = await _getMyReservationsHandler.HandleAsync(userId, CancellationToken.None);

            // Фильтрация
            var filtered = statusFilter switch
            {
                "Active" => all.Where(r => r.Status == ReservationStatus.Booked|| r.Status == ReservationStatus.Active),
                "Completed" => all.Where(r => r.Status == ReservationStatus.Completed),
                "Cancelled" => all.Where(r => r.Status == ReservationStatus.Cancelled),
                _ => all
            };

            // Сортировка
            Reservations = (sortField, sortAsc) switch
            {
                ("CourtName", true) => filtered.OrderBy(r => r.CourtName),
                ("CourtName", false) => filtered.OrderByDescending(r => r.CourtName),
                ("StartTime", true) => filtered.OrderBy(r => r.StartTime),
                ("StartTime", false) => filtered.OrderByDescending(r => r.StartTime),
                ("EndTime", true) => filtered.OrderBy(r => r.EndTime),
                ("EndTime", false) => filtered.OrderByDescending(r => r.EndTime),
                ("TotalCost", true) => filtered.OrderBy(r => r.TotalCost),
                ("TotalCost", false) => filtered.OrderByDescending(r => r.TotalCost),
                ("Status", true) => filtered.OrderBy(r => r.Status),
                ("Status", false) => filtered.OrderByDescending(r => r.Status),
                _ => filtered.OrderBy(r => r.StartTime)
            };

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
