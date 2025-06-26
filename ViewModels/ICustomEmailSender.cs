using System.Threading.Tasks;
namespace PakProperties.ViewModels
{
    public interface ICustomEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
