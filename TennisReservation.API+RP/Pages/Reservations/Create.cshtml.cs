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
using TennisReservation.Domain.Models;

namespace TennisReservation.API_RP.Pages.Reservations
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
        public CreateReservationCommand Command { get; set; }

        public List<TennisCourtDto> TennisCourts { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();

        // JSON с ценами для JavaScript
        public string CourtPricesJson { get; set; } = "{}";

        public async Task<IActionResult> OnGetAsync()
        {
            // Загружаем список кортов
            var courts = await _getAllTennisCourtsHandler.HandleAsync(CancellationToken.None);
            TennisCourts = courts.Select(c => new TennisCourtDto(
                c.Id,
                c.Name,
                c.HourlyRate,
                c.Description
            )).ToList();

            var users = await _getAllUsersHandler.HandleAsync(CancellationToken.None);
            Users = users.Select (u => new UserDto(
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.RegistrationDate,
                u.ReservationsCount)).ToList();


            // Создаем словарь цен для JavaScript
            var prices = TennisCourts.ToDictionary(
                c => c.Id.ToString(),
                c => c.HourlyRate
            );
            CourtPricesJson = JsonSerializer.Serialize(prices);

            // Устанавливаем значения по умолчанию
            Command = new CreateReservationCommand(
                Guid.Empty,
                Guid.Empty,
                DateTime.Now.Date.AddDays(1).AddHours(8),
                DateTime.Now.Date.AddDays(1).AddHours(9)
            );

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