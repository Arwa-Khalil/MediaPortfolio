using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        _logger.LogInformation("STUB: SendPasswordResetEmailAsync called for {Email} with token {Token}", toEmail, resetToken);
        return Task.CompletedTask;
    }
}
