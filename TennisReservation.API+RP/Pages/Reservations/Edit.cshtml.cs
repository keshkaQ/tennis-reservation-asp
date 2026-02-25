using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Application.Reservations.Queries;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Contracts.Reservations.Command;
using TennisReservation.Contracts.Reservations.Queries;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Domain.Enums;

namespace TennisReservation.API_RP.Pages.Reservations
{
    public class EditModel : PageModel
    {
        private readonly UpdateReservationHandler _updateReservationHandler;
        private readonly GetReservationByIdHandler _getReservationByIdHandler;
        private readonly GetAllTennisCourtsHandler _getAllTennisCourtsHandler;
        private readonly GetAllUsersHandler _getAllUsersHandler;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            UpdateReservationHandler updateReservationHandler,
            GetReservationByIdHandler getReservationByIdHandler,
            GetAllTennisCourtsHandler getAllTennisCourtsHandler,
            GetAllUsersHandler getAllUsersHandler,
            ILogger<EditModel> logger)
        {
            _updateReservationHandler = updateReservationHandler;
            _getReservationByIdHandler = getReservationByIdHandler;
            _getAllTennisCourtsHandler = getAllTennisCourtsHandler;
            _getAllUsersHandler = getAllUsersHandler;
            _logger = logger;
        }

        [BindProperty]
        public UpdateReservationCommand Command { get; set; }

        public List<TennisCourtDto> TennisCourts { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
        public string CourtPricesJson { get; set; } = "{}";

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                // Загружаем бронирование
                var reservationResult = await _getReservationByIdHandler.HandleAsync(
                    new GetReservationByIdQuery(id), CancellationToken.None);

                if (reservationResult.IsFailure)
                {
                    TempData["ErrorMessage"] = "Бронирование не найдено";
                    return RedirectToPage("./Index");
                }

                var reservation = reservationResult.Value;

                // Загружаем списки для выпадающих меню
                await LoadListsAsync();

                // Создаем команду с данными бронирования
                Command = new UpdateReservationCommand(
                    reservation.Id,
                    reservation.CourtId,
                    reservation.UserId,
                    reservation.StartTime,
                    reservation.EndTime
                );

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке бронирования {ReservationId}", id);
                TempData["ErrorMessage"] = "Ошибка при загрузке данных";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadListsAsync();
                return Page();
            }

            try
            {
                var result = await _updateReservationHandler.HandleAsync(Command, CancellationToken.None);

                if (result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, result.Error);
                    await LoadListsAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Бронирование успешно обновлено";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении бронирования {ReservationId}", Command.Id);
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных");
                await LoadListsAsync();
                return Page();
            }
        }

        private async Task LoadListsAsync()
        {
            var courts = await _getAllTennisCourtsHandler.HandleAsync(CancellationToken.None);
            TennisCourts = courts.Select(c => new TennisCourtDto(
                c.Id, c.Name, c.HourlyRate, c.Description
            )).ToList();

            var users = await _getAllUsersHandler.HandleAsync(CancellationToken.None);
            Users = users.ToList();

            // Для JavaScript
            var prices = TennisCourts.ToDictionary(c => c.Id.ToString(), c => c.HourlyRate);
            CourtPricesJson = JsonSerializer.Serialize(prices);
        }
    }
}