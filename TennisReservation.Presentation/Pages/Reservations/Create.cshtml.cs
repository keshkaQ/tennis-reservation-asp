using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.Users.Dto;

namespace TennisReservation.Presentation.Pages.Reservations
{
    public class CreateModel : PageModel
    {
        private readonly CreateReservationHandler _createReservationHandler;
        private readonly GetAllTennisCourtsHandler _getAllTennisCourtsHandler;
        private readonly GetAllUsersHandler _getAllUsersHandler;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            CreateReservationHandler createReservationHandler,
            GetAllTennisCourtsHandler getAllTennisCourtsHandler,
            GetAllUsersHandler getAllUsersHandler,
            ILogger<CreateModel> logger)
        {
            _createReservationHandler = createReservationHandler;
            _getAllTennisCourtsHandler = getAllTennisCourtsHandler;
            _getAllUsersHandler = getAllUsersHandler;
            _logger = logger;
        }

        [BindProperty]
        public CreateReservationCommand Command { get; set; } = new(
            TennisCourtId: Guid.Empty,
            UserId: Guid.Empty,
            StartTime: DateTime.Today.AddHours(10),
            EndTime: DateTime.Today.AddHours(11)
        );

        public List<TennisCourtDto> TennisCourts { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();

        // JSON с ценами для JavaScript
        public string CourtPricesJson { get; set; } = "{}";

        public async Task<IActionResult> OnGetAsync()
        {
            // Загружаем список кортов
            var courtsResult = await _getAllTennisCourtsHandler.HandleAsync(CancellationToken.None);
            TennisCourts = courtsResult.Value?.ToList() ?? new List<TennisCourtDto>();

            var usersResult = await _getAllUsersHandler.HandleAsync(CancellationToken.None);
            Users = usersResult.Value?.ToList() ?? new List<UserDto>();

            // Создаем словарь цен для JavaScript
            var prices = TennisCourts.ToDictionary(
                c => c.Id.ToString(),
                c => c.HourlyRate
            );
            CourtPricesJson = JsonSerializer.Serialize(prices);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var result = await _createReservationHandler.HandleAsync(Command, CancellationToken.None);
                if (result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, result.Error);
                    return Page();
                }
                TempData["SuccessMessage"] = "Бронирование успешно добавлено";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении бронирования");
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных бронирования");
                return Page();
            }
        }
    }
}