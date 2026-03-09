using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.TennisCourts.Commands.CreateTennisCourt;
using TennisReservation.Presentation.Pages.TennisCourts.ViewModels;

namespace TennisReservation.Presentation.Pages.TennisCourts
{
    public class CreateModel : PageModel
    {
        private readonly CreateTennisCourtHandler _createTennisCourtHandler;

        public CreateModel(CreateTennisCourtHandler createTennisCourtHandler)
        {
            _createTennisCourtHandler = createTennisCourtHandler;
        }

        [BindProperty]
        public CreateTennisCourtViewModel ViewModel { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var command = new CreateTennisCourtCommand(
                ViewModel.Name,
                ViewModel.HourlyRate,
                ViewModel.Description ?? string.Empty);

            var result = await _createTennisCourtHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return Page();
            }

            TempData["SuccessMessage"] = $"Корт '{ViewModel.Name}' успешно создан";
            return RedirectToPage("./Index");
        }
    }
}