using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Presentation.Pages.TennisCourts.ViewModels;

namespace TennisReservation.Presentation.Pages.TennisCourts
{
    public class CreateModel : PageModel
    {
        private readonly CreateTennisCourtHandler _createTennisCourtHandler;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            CreateTennisCourtHandler createTennisCourtHandler,
            ILogger<CreateModel> logger)
        {
            _createTennisCourtHandler = createTennisCourtHandler;
            _logger = logger;
        }

        [BindProperty]
        public CreateTennisCourtViewModel ViewModel { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Маппинг из ViewModel в Command
                var command = new CreateTennisCourtCommand(
                    ViewModel.Name,
                    ViewModel.HourlyRate,
                    ViewModel.Description ?? string.Empty
                );

                var result = await _createTennisCourtHandler.HandleAsync(command, CancellationToken.None);

                if (result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, result.Error);
                    return Page();
                }

                TempData["SuccessMessage"] = $"Корт '{ViewModel.Name}' успешно создан";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании корта");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при создании корта");
                return Page();
            }
        }
    }
}