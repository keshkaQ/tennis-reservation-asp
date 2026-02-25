using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Contracts.Users.Commands;

namespace TennisReservation.API_RP.Pages.Users
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
        public CreateUserCommand Command { get; set; }

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
                var result = await _createUserHandler.HandleAsync(Command, CancellationToken.None);
                if(result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при создании пользователя");
                    return Page();
                }

                TempData["SuccessMessage"] = $"Клиент {Command.FirstName} {Command.LastName} успешно добавлен";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Clients_Email"))
                {
                    ModelState.AddModelError("Client.Email", "Клиент с таким email уже существует");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных пользователя");
                }

                return Page();
            }
        }
    }
}