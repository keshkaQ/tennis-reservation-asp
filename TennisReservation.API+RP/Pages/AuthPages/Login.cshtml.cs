using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TennisReservation.API_RP.Pages.AuthPages;

public class LoginModel : PageModel
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public string? ReturnUrl { get; set; }
    public void OnGet()
    {
    }
}
