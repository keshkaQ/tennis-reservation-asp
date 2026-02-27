using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Auth;
using TennisReservation.Presentation.Pages.AuthPages.ViewModels;

namespace TennisReservation.Presentation.Pages.AuthPages;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; }

    private readonly ILogger<LoginModel> _logger;
    private readonly UserService _userService;
    public bool ShowLoginError { get; set; }

    public LoginModel(ILogger<LoginModel> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Пользователь уже аутентифицирован, перенаправление на главную");
                return RedirectToPage("/Index");
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке аутентификации на странице входа");
            return RedirectToPage("/Error", new { code = 500 });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ShowLoginError = false;

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Невалидная модель при попытке входа для email: {Email}", Input.Email);
            return Page();
        }

        try
        {
            var token = await _userService.Login(Input.Email!, Input.Password!);

            if (token == null)
            {
                _logger.LogWarning("Неудачная попытка входа для email: {Email}", Input.Email);
                ShowLoginError = true;
                Input.Password = string.Empty;
                return Page();
            }

            _logger.LogInformation("Пользователь {Email} успешно вошел в систему", Input.Email);

            HttpContext.Response.Cookies.Append("jwt-cookies", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(12)
            });

            TempData["SuccessMessage"] = "Добро пожаловать!";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при входе пользователя {Email}", Input.Email);

            ShowLoginError = true;
            return Page();
        }
    }
}