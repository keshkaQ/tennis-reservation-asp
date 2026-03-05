using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.Queries;
using TennisReservation.Presentation.Pages.TennisCourts.ViewModels;

namespace TennisReservation.Presentation.Pages.TennisCourts
{
    public class EditModel : PageModel
    {
        private readonly UpdateTennisCourtHandler _updateTennisCourtHandler;
        private readonly GetTennisCourtByIdHandler _getTennisCourtByIdHandler;

        public EditModel(
            UpdateTennisCourtHandler updateTennisCourtHandler,
            GetTennisCourtByIdHandler getTennisCourtByIdHandler)
        {
            _updateTennisCourtHandler = updateTennisCourtHandler;
            _getTennisCourtByIdHandler = getTennisCourtByIdHandler;
        }

        [BindProperty]
        public EditTennisCourtViewModel ViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var tennisCourtToUpdate = await _getTennisCourtByIdHandler.HandleAsync(
                new GetTennisCourtByIdQuery(id), CancellationToken.None);

            if (tennisCourtToUpdate == null)
                return NotFound();

            ViewModel.Id = tennisCourtToUpdate.Id;
            ViewModel.Name = tennisCourtToUpdate.Name;
            ViewModel.HourlyRate = tennisCourtToUpdate.HourlyRate;
            ViewModel.Description = tennisCourtToUpdate.Description;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (id != ViewModel.Id)
                return BadRequest("ID в маршруте не совпадает с ID модели");

            if (!ModelState.IsValid)
                return Page();

            var command = new UpdateTennisCourtCommand(
                ViewModel.Id,
                ViewModel.Name,
                ViewModel.HourlyRate,
                ViewModel.Description ?? string.Empty);

            var result = await _updateTennisCourtHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return Page();
            }

            TempData["SuccessMessage"] = $"{ViewModel.Name} успешно обновлен";
            return RedirectToPage("./Index");
        }
    }
}