using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.Queries;
using TennisReservation.Presentation.Pages.TennisCourts.ViewModels;

namespace TennisReservation.Presentation.Pages.TennisCourts
{
    public class EditModel : PageModel
    {
        private readonly UpdateTennisCourtHandler _updateTennisCourtHandler;
        private readonly GetTennisCourtByIdHandler _getTennisCourtByIdHandler;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            UpdateTennisCourtHandler updateTennisCourtHandler,
            GetTennisCourtByIdHandler getTennisCourtByIdHandler,
            ILogger<EditModel> logger)
        {
            _updateTennisCourtHandler = updateTennisCourtHandler;
            _getTennisCourtByIdHandler = getTennisCourtByIdHandler;
            _logger = logger;
        }

        [BindProperty]
        public EditTennisCourtViewModel ViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var tennisCourtToUpdate = await _getTennisCourtByIdHandler.HandleAsync(
                new GetTennisCourtByIdQuery(id),
                CancellationToken.None);

            if (tennisCourtToUpdate == null)
                return NotFound();

            // Маппинг из DTO в ViewModel
            ViewModel.Id = tennisCourtToUpdate.Id;
            ViewModel.Name = tennisCourtToUpdate.Name;
            ViewModel.HourlyRate = tennisCourtToUpdate.HourlyRate;
            ViewModel.Description = tennisCourtToUpdate.Description;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (id != ViewModel.Id)
            {
                return BadRequest("ID в маршруте не совпадает с ID модели");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Маппинг из ViewModel в Command
                var command = new UpdateTennisCourtCommand(
                    ViewModel.Id,
                    ViewModel.Name,
                    ViewModel.HourlyRate,
                    ViewModel.Description ?? string.Empty
                );

                var result = await _updateTennisCourtHandler.HandleAsync(command, CancellationToken.None);

                if (result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, result.Error);
                    return Page();
                }

                TempData["SuccessMessage"] = $"{ViewModel.Name} успешно обновлен";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка БД при обновлении корта");

                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_TennisCourts_Name"))
                {
                    ModelState.AddModelError("ViewModel.Name", "Корт с таким именем уже существует");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных корта");
                }

                return Page();
            }
        }
    }
}