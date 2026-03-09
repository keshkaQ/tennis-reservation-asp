using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Users.Auth;
using TennisReservation.Presentation.Pages.AuthPages.ViewModels;

namespace TennisReservation.Presentation.Pages.AuthPages;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; }

    private readonly ILogger<LoginModel> _logger;
    private readonly UserService _userService;
    public string? LoginErrorMessage { get; set; }

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
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var result = await _userService.Login(Input.Email!, Input.Password!);
            if (result.IsFailure)
            {
                LoginErrorMessage = result.Error; 
                Input.Password = string.Empty;
                return Page();
            }

            HttpContext.Response.Cookies.Append("jwt-cookies", result.Value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(12)
            });
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при входе {Email}", Input.Email);
            LoginErrorMessage = "Произошла ошибка. Попробуйте позже";
            return Page();
        }
    }
}