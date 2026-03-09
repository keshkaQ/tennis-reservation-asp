using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TennisReservation.Application.Reservations.Commands.CreateReservation;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Presentation.Pages.Reservations.ViewModels;

namespace TennisReservation.Presentation.Pages.Reservations
{
    public class CreateModel : PageModel
    {
        private readonly CreateReservationHandler _createReservationHandler;
        private readonly GetAllTennisCourtsHandler _getAllTennisCourtsHandler;
        private readonly GetAllUsersHandler _getAllUsersHandler;

        public CreateModel(
            CreateReservationHandler createReservationHandler,
            GetAllTennisCourtsHandler getAllTennisCourtsHandler,
            GetAllUsersHandler getAllUsersHandler)
        {
            _createReservationHandler = createReservationHandler;
            _getAllTennisCourtsHandler = getAllTennisCourtsHandler;
            _getAllUsersHandler = getAllUsersHandler;
        }

        [BindProperty]
        public CreateReservationViewModel ViewModel { get; set; } = new();

        public List<TennisCourtDto> TennisCourts { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
        public string CourtPricesJson { get; set; } = "{}";

        private async Task LoadDataAsync()
        {
            var courtsResult = await _getAllTennisCourtsHandler.HandleAsync(CancellationToken.None);
            TennisCourts = courtsResult.Value?.ToList() ?? new();

            var usersResult = await _getAllUsersHandler.HandleAsync(CancellationToken.None);
            Users = usersResult.Value?.ToList() ?? new();

            var prices = TennisCourts.ToDictionary(c => c.Id.ToString(), c => c.HourlyRate);
            CourtPricesJson = JsonSerializer.Serialize(prices);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(ViewModel.Date) || string.IsNullOrEmpty(ViewModel.StartTime))
            {
                TempData["Error"] = "Заполните все поля бронирования";
                return Page();
            }

            if (!DateTime.TryParse($"{ViewModel.Date} {ViewModel.StartTime}", out var startDateTime))
            {
                TempData["Error"] = "Некорректный формат даты или времени";
                return Page();
            }

            if (!double.TryParse(ViewModel.Duration,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var durationHours) || durationHours <= 0)
            {
                TempData["Error"] = "Некорректная длительность";
                return Page();
            }

            if (startDateTime < DateTime.Now)
            {
                TempData["Error"] = "Нельзя забронировать корт на прошедшее время";
                return Page();
            }

            var endDateTime = startDateTime.AddHours(durationHours);

            if (endDateTime.TimeOfDay > new TimeSpan(22, 30, 0))
            {
                TempData["Error"] = "Бронирование не может заканчиваться после 22:30";
                return Page();
            }

            if (endDateTime.Date != startDateTime.Date)
            {
                TempData["Error"] = "Бронирование не может переходить на следующий день";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var command = new CreateReservationCommand(
                ViewModel.TennisCourtId,
                ViewModel.UserId,
                startDateTime,
                endDateTime);

            var result = await _createReservationHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                TempData["Error"] = "Корт уже забронирован на это время";
                await LoadDataAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Бронирование успешно добавлено";
            return RedirectToPage("./Index");
        }
    }
}