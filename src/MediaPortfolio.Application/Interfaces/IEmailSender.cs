using System.Threading.Tasks;

namespace MediaPortfolio.Application.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
}
