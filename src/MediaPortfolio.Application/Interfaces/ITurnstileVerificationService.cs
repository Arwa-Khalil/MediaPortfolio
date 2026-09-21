using System.Threading;
using System.Threading.Tasks;

namespace MediaPortfolio.Application.Interfaces;

public interface ITurnstileVerificationService
{
    Task<bool> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default);
}
