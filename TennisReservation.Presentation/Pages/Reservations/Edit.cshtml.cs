using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TennisReservation.Application.Reservations.Commands.UpdateReservation;
using TennisReservation.Application.Reservations.Queries.GetReservationById;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Presentation.Pages.Reservations.ViewModels;

namespace TennisReservation.Presentation.Pages.Reservations
{
    public class EditModel : PageModel
    {
        private readonly UpdateReservationHandler _updateReservationHandler;
        private readonly GetReservationByIdHandler _getReservationByIdHandler;
        private readonly GetAllTennisCourtsHandler _getAllTennisCourtsHandler;
        private readonly GetAllUsersHandler _getAllUsersHandler;

        public EditModel(
            UpdateReservationHandler updateReservationHandler,
            GetReservationByIdHandler getReservationByIdHandler,
            GetAllTennisCourtsHandler getAllTennisCourtsHandler,
            GetAllUsersHandler getAllUsersHandler)
        {
            _updateReservationHandler = updateReservationHandler;
            _getReservationByIdHandler = getReservationByIdHandler;
            _getAllTennisCourtsHandler = getAllTennisCourtsHandler;
            _getAllUsersHandler = getAllUsersHandler;
        }

        [BindProperty]
        public EditReservationViewModel ViewModel { get; set; } = new();

        public List<TennisCourtDto> TennisCourts { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
        public string CourtPricesJson { get; set; } = "{}";

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var reservationResult = await _getReservationByIdHandler.HandleAsync(
                new GetReservationByIdQuery(id), CancellationToken.None);

            if (reservationResult.IsFailure)
            {
                TempData["ErrorMessage"] = "Бронирование не найдено";
                return RedirectToPage("./Index");
            }

            var reservation = reservationResult.Value;
            await LoadListsAsync();

            ViewModel.TennisCourtId = reservation.CourtId;
            ViewModel.UserId = reservation.UserId;
            ViewModel.StartTime = reservation.StartTime;
            ViewModel.EndTime = reservation.EndTime;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            ViewModel.Id = id;

            if (!ModelState.IsValid)
            {
                await LoadListsAsync();
                return Page();
            }
            var currentUserId = Guid.Parse(User.FindFirst("userId")?.Value);
            var isAdmin = User.IsInRole("Admin");

            var command = new UpdateReservationCommand(
                ViewModel.Id,
                ViewModel.TennisCourtId,
                currentUserId,
                isAdmin, 
                ViewModel.StartTime,
                ViewModel.EndTime);

            var result = await _updateReservationHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                await LoadListsAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Бронирование успешно обновлено";
            return RedirectToPage("./Index");
        }

        private async Task LoadListsAsync()
        {
            var courts = await _getAllTennisCourtsHandler.HandleAsync(CancellationToken.None);
            TennisCourts = courts.Value.Select(c => new TennisCourtDto(
                c.Id, c.Name, c.HourlyRate, c.Description
            )).ToList();

            var users = await _getAllUsersHandler.HandleAsync(CancellationToken.None);
            Users = users.Value.ToList();

            var prices = TennisCourts.ToDictionary(c => c.Id.ToString(), c => c.HourlyRate);
            CourtPricesJson = JsonSerializer.Serialize(prices);
        }
    }
}