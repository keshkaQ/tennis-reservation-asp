using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.TennisCourts.Queries;
using TennisReservation.Contracts.Reservations.Command;

namespace TennisReservation.Presentation.Pages.Reservations
{
    [Authorize]
    public class BookModel : PageModel
    {
        private readonly GetTennisCourtByIdHandler _courtHandler;
        private readonly CreateReservationHandler _createHandler;
        private readonly ILogger<BookModel> _logger;

        public TennisCourtDto? Court { get; set; }

        public BookModel(
            GetTennisCourtByIdHandler courtHandler,
            CreateReservationHandler createHandler,
            ILogger<BookModel> logger)
        {
            _courtHandler = courtHandler;
            _createHandler = createHandler;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(Guid courtId)
        {
            Court = await _courtHandler.HandleAsync(
                new GetTennisCourtByIdQuery(courtId),
                CancellationToken.None);

            if (Court == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            Guid courtId,
            string Date,
            string StartTime,
            string Duration)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (userId == null)
                return RedirectToPage("/AuthPages/Login");
            if (string.IsNullOrEmpty(Date) || string.IsNullOrEmpty(StartTime))
            {
                TempData["Error"] = "Заполните все поля бронирования";
                return RedirectToPage(new { courtId });
            }

            if (!DateTime.TryParse($"{Date} {StartTime}", out var startDateTime))
            {
                TempData["Error"] = "Некорректный формат даты или времени";
                return RedirectToPage(new { courtId });
            }

            if (!double.TryParse(Duration,
    System.Globalization.NumberStyles.Any,
    System.Globalization.CultureInfo.InvariantCulture, out var durationHours) || durationHours <= 0)
            {
                TempData["Error"] = "Некорректная длительность";
                return RedirectToPage(new { courtId });
            }


            if (startDateTime < DateTime.Now)
            {
                TempData["Error"] = "Нельзя забронировать корт на прошедшее время";
                return RedirectToPage(new { courtId });
            }

            var endDateTime = startDateTime.AddHours(durationHours);

            if (endDateTime.TimeOfDay > new TimeSpan(22, 30, 0))
            {
                TempData["Error"] = "Бронирование не может заканчиваться после 22:30";
                return RedirectToPage(new { courtId });
            }

            if (endDateTime.Date != startDateTime.Date)
            {
                TempData["Error"] = "Бронирование не может переходить на следующий день";
                return RedirectToPage(new { courtId });
            }

            try
            {
                var command = new CreateReservationCommand(
                    courtId,
                    Guid.Parse(userId),
                    startDateTime,
                    endDateTime);

                var result = await _createHandler.HandleAsync(command, CancellationToken.None);

                if (result.IsFailure)
                {
                    TempData["Error"] = result.Error;
                    return RedirectToPage(new { courtId });
                }

                TempData["Success"] = "Бронирование успешно создано!";
                return RedirectToPage(new { courtId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при бронировании корта {CourtId}", courtId);
                TempData["Error"] = "Ошибка при создании бронирования";
                return RedirectToPage(new { courtId });
            }
        }
    }
}