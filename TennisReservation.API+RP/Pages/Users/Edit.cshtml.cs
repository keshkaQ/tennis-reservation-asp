using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Queries;

namespace TennisReservation.API_RP.Pages.Users
{
    public class EditModel : PageModel
    {
        private readonly UpdateUserHandler _updateUserHandler;
        private readonly GetUserByIdHandler _getUserByIdHandler;
        private readonly ILogger<EditModel> _logger;
        public EditModel(UpdateUserHandler updateUserHandler, GetUserByIdHandler getUserByIdHandler, ILogger<EditModel> logger)
        {
            _updateUserHandler = updateUserHandler;
            _getUserByIdHandler = getUserByIdHandler;
            _logger = logger;
        }

        [BindProperty]
        public UpdateUserCommand Command { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userToUpdate = await _getUserByIdHandler.Handle(new GetUserByIdQuery(id), CancellationToken.None);
            if (userToUpdate == null)
                return NotFound();

            Command = new UpdateUserCommand(
                userToUpdate.FirstName,
                userToUpdate.LastName,
                userToUpdate.Email,
                userToUpdate.PhoneNumber
            );

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                var result = await _updateUserHandler.HandleAsync(id,Command, CancellationToken.None);
                if (result.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, result.Error);
                    return Page();
                }

                TempData["SuccessMessage"] = $"Клиент {Command.FirstName} {Command.LastName} успешно обновлен";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка БД при обновлении пользователя");
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Clients_Email"))
                {
                    ModelState.AddModelError("Command.Email", "Клиент с таким email уже существует");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ошибка при сохранении данных");
                }

                return Page();
            }
        }
    }
}