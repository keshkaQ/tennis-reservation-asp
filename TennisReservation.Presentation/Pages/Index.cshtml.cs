using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Contracts.TennisCourts.DTO;

namespace TennisReservation.Presentation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly GetAllTennisCourtsHandler _courtsHandler;
        private readonly ILogger<IndexModel> _logger;

        public List<TennisCourtDto> Courts { get; set; } = new();

        public IndexModel(
            GetAllTennisCourtsHandler courtsHandler,
            ILogger<IndexModel> logger)
        {
            _courtsHandler = courtsHandler;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Загружаем корты
                var courts = await _courtsHandler.HandleAsync(CancellationToken.None);
                Courts = courts.Value.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке главной страницы");
            }
        }
    }
}
