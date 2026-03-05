using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Presentation.Pages.Reservations
{
    public class IndexModel : PageModel
    {
        private readonly GetAllReservationsHandler _getAllReservationsHandler;
        private readonly DeleteReservationHandler _deleteReservationHandler;
        private readonly CancelReservationHandler _cancelReservationHandler;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            GetAllReservationsHandler getAllReservationsHandler,
            DeleteReservationHandler deleteReservationHandler,
            CancelReservationHandler cancelReservationHandler,
            ILogger<IndexModel> logger)
        {
            _getAllReservationsHandler = getAllReservationsHandler;
            _deleteReservationHandler = deleteReservationHandler;
            _cancelReservationHandler = cancelReservationHandler;
            _logger = logger;
        }

        public IEnumerable<ReservationListItemDto> Reservations { get; set; } = [];
        public string SortField { get; set; } = "StartTime";
        public bool SortAsc { get; set; } = true;
        public string StatusFilter { get; set; } = "All";

        public async Task OnGetAsync(
            string sortField = "StartTime",
            bool sortAsc = true,
            string statusFilter = "All")
        {
            SortField = sortField;
            SortAsc = sortAsc;
            StatusFilter = statusFilter;

            try
            {
                var all = await _getAllReservationsHandler.HandleAsync(CancellationToken.None);

                // Фильтрация по статусу
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

                _logger.LogDebug("Загружено {Count} бронирований, фильтр: {Filter}, сортировка: {Field} {Dir}",
                    Reservations.Count(), statusFilter, sortField, sortAsc ? "ASC" : "DESC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке списка бронирований");
                TempData["ErrorMessage"] = "Не удалось загрузить список бронирований";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var currentUserId = Guid.TryParse(User.FindFirst("userId")?.Value, out var parsedId)
                ? parsedId : (Guid?)null;
            var isAdmin = User.IsInRole("Admin");
            var command = new DeleteReservationCommand(id, currentUserId, isAdmin);
            var reservationResult = await _deleteReservationHandler.HandleAsync(command, CancellationToken.None);

            if (reservationResult.IsFailure)
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

            TempData["SuccessMessage"] = "Бронирование успешно удалено";
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
