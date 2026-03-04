using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Presentation.Pages.Reservations.ViewModels
{
    public class CreateReservationViewModel
    {
        [Required(ErrorMessage = "Выберите теннисный корт")]
        [Display(Name = "Теннисный корт")]
        public Guid TennisCourtId { get; set; }

        [Required(ErrorMessage = "Выберите клиента")]
        [Display(Name = "Клиент")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Укажите дату")]
        [Display(Name = "Начало")]
        public string Date {  get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите время начала")]
        [Display(Name = "Окончание")]
        public string StartTime {  get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите длительность")]
        [Display(Name = "Длительность")]
        public string Duration { get; set; } = string.Empty;
    }
}
