using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TennisReservation.Presentation.Pages
{
    public class ErrorModel : PageModel
    {
        public int? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDescription { get; set; }

        public void OnGet(int? code)
        {
            StatusCode = code ?? Response.StatusCode;

            switch (StatusCode)
            {
                case 404:
                    ErrorMessage = "Страница не найдена";
                    ErrorDescription = "Запрашиваемая страница не существует.";
                    break;
                case 403:
                    ErrorMessage = "Доступ запрещен";
                    ErrorDescription = "У вас нет прав для просмотра.";
                    break;
                case 400:
                    ErrorMessage = "Некорректный запрос";
                    ErrorDescription = "Проверьте правильность введенных данных.";
                    break;
                case 500:
                    ErrorMessage = "Ошибка сервера";
                    ErrorDescription = "Попробуйте позже.";
                    break;
                default:
                    ErrorMessage = "Ошибка";
                    ErrorDescription = "Что-то пошло не так.";
                    break;
            }
        }
    }
}