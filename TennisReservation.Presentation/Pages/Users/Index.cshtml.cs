using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Contracts.Users.Commands;
using TennisReservation.Contracts.Users.Dto;

namespace TennisReservation.Presentation.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly DeleteUserHandler _deleteUserHandler;
        private readonly GetAllUsersHandler _getAllUsersHandler;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            DeleteUserHandler deleteUserHandler,
            GetAllUsersHandler getAllUsersHandler,
            ILogger<IndexModel> logger)
        {
            _deleteUserHandler = deleteUserHandler;
            _getAllUsersHandler = getAllUsersHandler;
            _logger = logger;
        }

        public IEnumerable<UserDto> Users { get; set; } = [];
        public string SortField { get; set; } = "LastName";
        public bool SortAsc { get; set; } = true;

        public async Task OnGetAsync(string sortField = "LastName", bool sortAsc = true)
        {
            SortField = sortField;
            SortAsc = sortAsc;

            try
            {
                var result = await _getAllUsersHandler.HandleAsync(CancellationToken.None);
                var list = result.Value ?? new List<UserDto>();

                Users = (sortField, sortAsc) switch
                {
                    ("FirstName", true) => list.OrderBy(u => u.FirstName),
                    ("FirstName", false) => list.OrderByDescending(u => u.FirstName),
                    ("LastName", true) => list.OrderBy(u => u.LastName),
                    ("LastName", false) => list.OrderByDescending(u => u.LastName),
                    ("Email", true) => list.OrderBy(u => u.Email),
                    ("Email", false) => list.OrderByDescending(u => u.Email),
                    ("RegistrationDate", true) => list.OrderBy(u => u.RegistrationDate),
                    ("RegistrationDate", false) => list.OrderByDescending(u => u.RegistrationDate),
                    _ => list.OrderBy(u => u.LastName)
                };

                _logger.LogDebug("Загружено {Count} пользователей, сортировка: {Field} {Dir}",
                    Users.Count(), sortField, sortAsc ? "ASC" : "DESC");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке списка пользователей");
                TempData["ErrorMessage"] = "Не удалось загрузить список пользователей";
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var result = await _deleteUserHandler.HandleAsync(
                    new DeleteUserByIdCommand(id),
                    CancellationToken.None);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Не удалось удалить пользователя {UserId}: {Error}", id, result.Error);
                    TempData["ErrorMessage"] = result.Error;
                    return RedirectToPage();
                }

                _logger.LogInformation("Пользователь {UserId} успешно удален", id);
                TempData["SuccessMessage"] = "Пользователь успешно удален";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя {UserId}", id);
                TempData["ErrorMessage"] = "Не удалось удалить пользователя";
                return RedirectToPage();
            }
        }
    }
}
