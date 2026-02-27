using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Presentation.Pages.Users.ViewModels;

namespace TennisReservation.Presentation.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly CreateUserWithCredentialsHandler _createUserHandler;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(CreateUserWithCredentialsHandler createUserHandler, ILogger<CreateModel> logger)
        {
            _createUserHandler = createUserHandler;
            _logger = logger;
        }

        [BindProperty]
        public CreateUserViewModel ViewModel { get; set; } = new();

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
                var command = new CreateUserCommand(
                    ViewModel.FirstName,
                    ViewModel.LastName,
                    ViewModel.Email,
                    ViewModel.PhoneNumber,
                    ViewModel.Password);
                var result = await _createUserHandler.HandleAsync(command, CancellationToken.None);
                if(result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при создании пользователя");
                    _logger.LogCritical("Ошибка при создании пользователя");
                    return Page();
                }

                TempData["SuccessMessage"] = $"Клиент {command.FirstName} {command.LastName} успешно добавлен";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Clients_Email"))
                {
                    ModelState.AddModelError("Client.Email", "Клиент с таким email уже существует");
                    _logger.LogCritical("Клиент с таким email уже существует");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных пользователя");
                    _logger.LogCritical("Ошибка при сохранении данных пользователя");
                }

                return Page();
            }
        }
    }
}