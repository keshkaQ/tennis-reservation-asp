using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TennisReservation.Application.Auth;
using TennisReservation.Contracts.Users.Commands;

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

        public async Task<IActionResult> OnGetAsync() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var result = await _userService.Register(new CreateUserCommand(
                Input.FirstName,
                Input.LastName,
                Input.Email,
                Input.PhoneNumber,
                Input.Password
                ),CancellationToken.None);
            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return Page();
            }

            return RedirectToPage("/AuthPages/Login");
        }
    }

    public class RegisterInputModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 50 символов")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Введите корректный телефон")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; }
    }
}