using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Contracts.TennisCourts.Commands;
using TennisReservation.Contracts.TennisCourts.DTO;

namespace TennisReservation.API_RP.Pages.TennisCourts
{
    public class IndexModel : PageModel
    {
        private readonly DeleteTennisCourtHandler _deleteTennisCourtHandler;
        private readonly GetAllTennisCourtsHandler _getAllTennisCourtsHandler;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            DeleteTennisCourtHandler deleteTennisCourtHandler,
            GetAllTennisCourtsHandler getAllTennisCourtsHandler,
            ILogger<IndexModel> logger)
        {
            _deleteTennisCourtHandler = deleteTennisCourtHandler;
            _getAllTennisCourtsHandler = getAllTennisCourtsHandler;
            _logger = logger;
        }

        public IEnumerable<TennisCourtDto> TennisCourts { get; set; } = [];

        public async Task OnGetAsync()
        {
            try
            {
                var tennisCourts = await _getAllTennisCourtsHandler.HandleAsync(CancellationToken.None);
                TennisCourts = tennisCourts ?? new List<TennisCourtDto>();
                _logger.LogDebug("Загружено {Count} пользователей", TennisCourts.Count());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке списка кортов");
                TempData["ErrorMessage"] = "Не удалось загрузить список кортов";
            }
        }
        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var result = await _deleteTennisCourtHandler.HandleAsync(
                    new DeleteTennisCourtCommand(id),
                    CancellationToken.None);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Не удалось удалить корт {TennisCourtId}: {Error}", id, result.Error);
                    TempData["ErrorMessage"] = result.Error;
                    return RedirectToPage();
                }

                _logger.LogInformation("Корт {TennisCourtId} успешно удален", id);
                TempData["SuccessMessage"] = "Корт успешно удален";

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении корта {TennisCourtId}", id);
                TempData["ErrorMessage"] = "Не удалось удалить корт";
                return RedirectToPage();
            }
        }
    }
}