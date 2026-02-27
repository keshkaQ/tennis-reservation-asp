using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TennisReservation.Application.Auth;

namespace TennisReservation.Presentation.Pages.AuthPages;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; }
    private readonly ILogger<LoginModel> _logger;
    private readonly UserService _userService;
    public LoginModel(ILogger<LoginModel> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var token = await _userService.Login(Input.Email!, Input.Password!);

        if (token == null)
            return Page();

        HttpContext.Response.Cookies.Append("jwt-cookies", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(12)
        });

        return RedirectToPage("/Index");
    }
}

public class LoginInputModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
