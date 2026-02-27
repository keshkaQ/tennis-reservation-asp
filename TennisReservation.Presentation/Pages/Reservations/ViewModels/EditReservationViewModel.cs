using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Presentation.Pages.Reservations.ViewModels
{
    public class EditReservationViewModel
    {
        public Guid Id {  get; set; }

        [Required(ErrorMessage = "Выберите теннисный корт")]
        [Display(Name = "Теннисный корт")]
        public Guid TennisCourtId {  get; set; }

        [Required(ErrorMessage = "Выберите клиента")]
        [Display(Name = "Клиент")]
        public Guid UserId {  get; set; }

        [Required(ErrorMessage = "Укажите дату и время начала")]
        [Display(Name = "Начало")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime {  get; set; }

        [Required(ErrorMessage = "Укажите дату и время окончания")]
        [Display(Name = "Окончание")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime {  get; set; }
    }
}
