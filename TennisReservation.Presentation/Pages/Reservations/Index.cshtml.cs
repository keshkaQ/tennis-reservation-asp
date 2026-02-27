using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Domain.Models;
using TennisReservation.Infrastructure.Postgres;

namespace TennisReservation.Presentation.Pages.Reservations
{
    public class IndexModel : PageModel
    {
        private readonly TennisReservationDbContext _context;

        public IndexModel(TennisReservationDbContext context)
        {
            _context = context;
        }

        public IList<Reservation> Reservations { get; set; } = [];

        public async Task OnGetAsync()
        {
            Reservations = await _context.ReservationsRead
                .Include(b => b.User)
                .Include(b => b.TennisCourt)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var booking = await _context.Reservations.FindAsync(new ReservationId(id));

            if (booking != null)
            {
                _context.Reservations.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Броинрование успешно удалено";
            }
            else
            {
                TempData["ErrorMessage"] = "Бронирование не найдено";
            }

            return RedirectToPage();
        }
    }
}