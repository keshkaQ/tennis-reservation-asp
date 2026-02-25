using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TennisReservation.Application.Statictics;

namespace TennisReservation.API_RP.Pages
{
    public class IndexModel : PageModel
    {
        private readonly GetStatisticsHandler _statisticsHandler;

        public IndexModel(GetStatisticsHandler statisticsHandler)
        {
            _statisticsHandler = statisticsHandler;
        }
        public int UsersCount { get; set; }
        public int TennisCourtsCount { get; set; }
        public int ReservationsCount { get; set; }
        public int UserCredentialCount { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var stats = await _statisticsHandler.Handle(cancellationToken);
            UsersCount = stats.UsersCount;
            TennisCourtsCount = stats.TennisCourtsCount;
            ReservationsCount = stats.ReservationsCount;
            UserCredentialCount = stats.UserCredentialCount;
            return Page();
        }
    }
}
