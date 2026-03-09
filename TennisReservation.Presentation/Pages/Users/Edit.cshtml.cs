using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Users.Commands.UpdateUser;
using TennisReservation.Application.Users.Queries.GetUserById;
using TennisReservation.Presentation.Pages.Users.ViewModels;

namespace TennisReservation.Presentation.Pages.Users
{
    public class EditModel : PageModel
    {
        private readonly UpdateUserHandler _updateUserHandler;
        private readonly GetUserByIdHandler _getUserByIdHandler;

        public EditModel(UpdateUserHandler updateUserHandler, GetUserByIdHandler getUserByIdHandler)
        {
            _updateUserHandler = updateUserHandler;
            _getUserByIdHandler = getUserByIdHandler;
        }

        [BindProperty]
        public EditUserViewModel ViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userToUpdateResult = await _getUserByIdHandler.HandleAsync(new GetUserByIdQuery(id), CancellationToken.None);

            if (userToUpdateResult.IsFailure)
                return NotFound();

            var userToUpdate = userToUpdateResult.Value;
            ViewModel.Id = userToUpdate.UserId;
            ViewModel.FirstName = userToUpdate.FirstName;
            ViewModel.LastName = userToUpdate.LastName;
            ViewModel.Email = userToUpdate.Email;
            ViewModel.PhoneNumber = userToUpdate.PhoneNumber;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            if (id != ViewModel.Id)
                return BadRequest("ID в маршруте не совпадает с ID модели");

            if (!ModelState.IsValid)
                return Page();

            var command = new UpdateUserCommand(
                ViewModel.Id,
                ViewModel.FirstName,
                ViewModel.LastName,
                ViewModel.Email,
                ViewModel.PhoneNumber);

            var result = await _updateUserHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return Page();
            }

            TempData["SuccessMessage"] = $"Клиент {command.FirstName} {command.LastName} успешно обновлен";
            return RedirectToPage("./Index");
        }
    }
}