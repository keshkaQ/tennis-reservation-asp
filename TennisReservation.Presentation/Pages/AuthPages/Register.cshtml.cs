using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Users.Auth;
using TennisReservation.Application.Users.Commands.CreateUser;

namespace TennisReservation.Presentation.Pages.AuthPages
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(UserService userService, ILogger<RegisterModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; }
        public bool ShowRegistrationError { get; set; }
        public string? RegistrationErrorMessage { get; set; }
        public async Task<IActionResult> OnGetAsync() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            ShowRegistrationError = false;
            RegistrationErrorMessage = null;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _userService.Register(new CreateUserWithCredentialsCommand(
                    Input.FirstName,
                    Input.LastName,
                    Input.Email,
                    Input.PhoneNumber,
                    Input.Password
                ), CancellationToken.None);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Регистрация пользователя {Email} не удалась: {Error}",
                        Input.Email, result.Error);
                    ShowRegistrationError = true;
                    RegistrationErrorMessage = result.Error;
                    Input.Password = string.Empty;
                    Input.ConfirmPassword = string.Empty;

                    return Page();
                }

                _logger.LogInformation("Пользователь {Email} успешно зарегистрирован", Input.Email);
                TempData["SuccessMessage"] = "Регистрация прошла успешно! Теперь вы можете войти.";
                return RedirectToPage("/AuthPages/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при регистрации пользователя {Email}", Input.Email);
                ShowRegistrationError = true;
                RegistrationErrorMessage = "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.";

                return Page();
            }
        }
    }
}