using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.Commands;

namespace TennisReservation.Presentation.Pages.TennisCourts
{
    public class CreateModel : PageModel
    {
        private readonly CreateTennisCourtHandler _createTennisCourtHandler;
        private readonly ILogger<CreateModel> _logger;
        public CreateModel(CreateTennisCourtHandler createTennisCourtHandler, ILogger<CreateModel> logger)
        {
            _createTennisCourtHandler = createTennisCourtHandler;
            _logger = logger;
        }

        [BindProperty]
        public CreateTennisCourtCommand Command { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var result = await _createTennisCourtHandler.HandleAsync(Command, CancellationToken.None);
                if (result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при создании корта");
                    return Page();
                }


                TempData["SuccessMessage"] = $"{Command.Name} успешно добавлен";
                return RedirectToPage("./Index");
            }
            catch(DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_TennisCourts_Name"))
                {
                    ModelState.AddModelError("TennisCourt.Name", "Корт с таким именем уже существует");
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
