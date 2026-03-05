using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TennisReservation.Presentation.Pages.AuthPages;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        Response.Cookies.Delete("jwt-cookies", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
        _logger.LogInformation("Пользователь вышел из системы");
        return RedirectToPage("/Index");
    }
}