using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.Queries;

namespace TennisReservation.API_RP.Pages.TennisCourts
{
    public class EditModel : PageModel
    {
        private readonly UpdateTennisCourtHandler _updateTennisCourtHandler;
        private readonly GetTennisCourtByIdHandler _getTennisCourtByIdHandler;
        private readonly ILogger<EditModel> _logger;
        public EditModel(UpdateTennisCourtHandler updateTennisCourtHandler,
            GetTennisCourtByIdHandler getTennisCourtByIdHandler,
            ILogger<EditModel> logger)
        {
            _updateTennisCourtHandler = updateTennisCourtHandler;
            _getTennisCourtByIdHandler = getTennisCourtByIdHandler;
            _logger = logger;
        }

        [BindProperty]
        public UpdateTennisCourtCommand Command { get; set; }
        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var tennisCourtToUpdate = await _getTennisCourtByIdHandler.HandleAsync(
                new GetTennisCourtByIdQuery(id),
                CancellationToken.None);
            if(tennisCourtToUpdate == null)
                return NotFound();
            Command = new UpdateTennisCourtCommand(
                Name: tennisCourtToUpdate.Name,
                HourlyRate: tennisCourtToUpdate.HourlyRate,
                Description: tennisCourtToUpdate.Description
            );

            return Page();
        }
        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                var result = await _updateTennisCourtHandler.HandleAsync(id,Command, CancellationToken.None);
                if (result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, result.Error);
                    return Page();
                }
                TempData["SuccessMessage"] = $"{Command.Name} успешно обновлен";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка БД при обновлении корта");
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_TennisCourts_Name"))
                {
                    ModelState.AddModelError("Command.Name", "Корт с таким именем уже существует");
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
