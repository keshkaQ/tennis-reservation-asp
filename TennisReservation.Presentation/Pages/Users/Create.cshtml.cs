using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Presentation.Pages.Users.ViewModels;

namespace TennisReservation.Presentation.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly CreateUserWithCredentialsHandler _createUserHandler;

        public CreateModel(CreateUserWithCredentialsHandler createUserHandler)
        {
            _createUserHandler = createUserHandler;
        }

        [BindProperty]
        public CreateUserViewModel ViewModel { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var command = new CreateUserCommand(
                ViewModel.FirstName,
                ViewModel.LastName,
                ViewModel.Email,
                ViewModel.PhoneNumber,
                ViewModel.Password);

            var result = await _createUserHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return Page();
            }

            TempData["SuccessMessage"] = $"Клиент {command.FirstName} {command.LastName} успешно добавлен";
            return RedirectToPage("./Index");
        }
    }
}