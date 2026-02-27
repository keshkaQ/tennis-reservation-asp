using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
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

        public async Task OnGetAsync()
        {

            try
            {
                var users = await _getAllUsersHandler.HandleAsync(CancellationToken.None);
                Users = users.Value ?? new List<UserDto>();

                _logger.LogDebug("Загружено {Count} пользователей", Users.Count());
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