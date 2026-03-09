using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Users.Commands.ChangePassword;
using TennisReservation.Application.Users.Commands.ChangeRole;
using TennisReservation.Application.Users.Commands.LockUser;
using TennisReservation.Application.Users.Commands.UnlockUser;
using TennisReservation.Application.Users.Interfaces;
using TennisReservation.Domain.Models;
using TennisReservation.Presentation.Pages.Users.ViewModels;

namespace TennisReservation.Presentation.Pages.Users
{
    public class ManageCredentialsModel : PageModel
    {
        private readonly ChangeRoleHandler _changeRoleHandler;
        private readonly ChangePasswordHandler _changePasswordHandler;
        private readonly LockUserHandler _lockUserHandler;
        private readonly UnlockUserHandler _unlockUserHandler;
        private readonly IUserCredentialsRepository _userCredentialsRepository;
        public ManageCredentialsModel(ChangeRoleHandler changeRoleHandler, ChangePasswordHandler changePasswordHandler, LockUserHandler lockUserHandler, UnlockUserHandler unlockUserHandler, IUserCredentialsRepository userCredentialsRepository, ILogger<ManageCredentialsModel> logger)
        {
            _changeRoleHandler = changeRoleHandler;
            _changePasswordHandler = changePasswordHandler;
            _lockUserHandler = lockUserHandler;
            _unlockUserHandler = unlockUserHandler;
            _userCredentialsRepository = userCredentialsRepository;
        }

        public UserCredentials? UserInfo { get; set; }

        [BindProperty]
        public ManageCredentialsViewModel ViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userResult = await _userCredentialsRepository.GetWithUserByIdAsync(new UserId(id).Value);
            if (userResult.IsFailure || userResult.Value == null)
                return NotFound();
            UserInfo = userResult.Value;
            ViewModel.UserId = id;
            return Page();
        }
        public async Task<IActionResult> OnPostChangeRoleAsync()
        {
            var currentUserId = User.FindFirst("userId")?.Value;
            if (currentUserId != null && ViewModel.UserId == Guid.Parse(currentUserId))
            {
                TempData["ErrorMessage"] = "Нельзя изменить роль самому себе";
                return RedirectToPage(new { id = ViewModel.UserId });
            }

            var command = new ChangeRoleCommand(ViewModel.UserId, ViewModel.Role);
            var result = await _changeRoleHandler.HandleAsync(command, CancellationToken.None);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
                result.IsSuccess ? "Роль успешно изменена" : result.Error;

            return RedirectToPage(new { id = ViewModel.UserId });
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState["ViewModel.NewPassword"]?.Errors.FirstOrDefault()?.ErrorMessage;
                TempData["ErrorMessage"] = error ?? "Некорректные данные";
                return RedirectToPage(new { id = ViewModel.UserId });
            }
            if (string.IsNullOrWhiteSpace(ViewModel.ConfirmPassword) || string.IsNullOrWhiteSpace(ViewModel.NewPassword))
            {
                TempData["ErrorMessage"] = "Заполните поля 'Новый пароль' и 'Подтверждение'";
                return RedirectToPage(new { id = ViewModel.UserId });
            }
            if(ViewModel.ConfirmPassword!= ViewModel.NewPassword)
            {
                TempData["ErrorMessage"] = "Пароли не совпадают";
                return RedirectToPage(new { id = ViewModel.UserId });
            }
            var command = new ChangePasswordCommand(ViewModel.UserId, ViewModel.NewPassword);
            var result = await _changePasswordHandler.HandleAsync(command, CancellationToken.None);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
                result.IsSuccess ? "Пароль успешно изменён" : result.Error;

            return RedirectToPage(new { id = ViewModel.UserId });
        }

        public async Task<IActionResult> OnPostLockAsync()
        {
            if (ViewModel.LockUntil <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Укажите дату в будущем";
                return RedirectToPage(new { id = ViewModel.UserId });
            }
            var currentUserId = User.FindFirst("userId")?.Value;
            if (currentUserId != null && ViewModel.UserId == Guid.Parse(currentUserId))
            {
                TempData["ErrorMessage"] = "Нельзя заблокировать самого себя";
                return RedirectToPage(new { id = ViewModel.UserId });
            }
            var command = new LockUserCommand(ViewModel.UserId,
                    DateTime.SpecifyKind(ViewModel.LockUntil, DateTimeKind.Utc));
            var result = await _lockUserHandler.HandleAsync(command, CancellationToken.None);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
                result.IsSuccess ? "Блокировка пользователя прошла успешно" : result.Error;

            return RedirectToPage(new { id = ViewModel.UserId });
        }
        public async Task<IActionResult> OnPostUnlockAsync()
        {
            var command = new UnlockUserCommand(ViewModel.UserId);
            var result = await _unlockUserHandler.HandleAsync(command, CancellationToken.None);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] =
                result.IsSuccess ? "Блокировка пользователя успешно снята" : result.Error;
            return RedirectToPage(new { id = ViewModel.UserId });
        }
    }
}
