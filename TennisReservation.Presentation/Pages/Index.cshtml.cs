using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Reservations.Queries;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Contracts.TennisCourts.DTO;

namespace TennisReservation.Presentation.Pages
{
    public class TimeSlot
    {
        public Guid CourtId { get; set; }
        public string CourtName { get; set; } = "";
        public DateTime Time { get; set; }
        public bool IsFree { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly GetAllTennisCourtsHandler _courtsHandler;
        private readonly GetAllReservationsHandler _reservationsHandler;
        private readonly ILogger<IndexModel> _logger;

        public List<TennisCourtDto> Courts { get; set; } = new();
        public List<TimeSlot> TodaySlots { get; set; } = new();

        public IndexModel(
            GetAllTennisCourtsHandler courtsHandler,
            GetAllReservationsHandler reservationsHandler,
            ILogger<IndexModel> logger)
        {
            _courtsHandler = courtsHandler;
            _reservationsHandler = reservationsHandler;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Загружаем корты
                var courts = await _courtsHandler.HandleAsync(CancellationToken.None);
                Courts = courts.Value.ToList();

                // Загружаем брони на сегодня
                var reservations = await _reservationsHandler.HandleAsync(CancellationToken.None);
                var today = DateTime.Today;
                var todayReservations = reservations
                    .Where(r => r.StartTime.Date == today)
                    .ToList();

                // Генерируем слоты с 8:00 до 22:00 по каждому корту
                var slots = new List<TimeSlot>();
                foreach (var court in Courts)
                {
                    for (int hour = 8; hour < 22; hour++)
                    {
                        var slotTime = today.AddHours(hour);
                        var isBusy = todayReservations.Any(r =>
                            r.CourtId == court.Id &&
                            r.StartTime <= slotTime &&
                            r.EndTime > slotTime);

                        slots.Add(new TimeSlot
                        {
                            CourtId = court.Id,
                            CourtName = court.Name,
                            Time = slotTime,
                            IsFree = !isBusy
                        });
                    }
                }

                // Показываем только слоты начиная с текущего часа
                TodaySlots = slots
                    .Where(s => s.Time >= DateTime.Now.AddMinutes(-30))
                    .OrderBy(s => s.Time)
                    .Take(28) // не перегружать страницу
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке главной страницы");
            }
        }
    }
}
