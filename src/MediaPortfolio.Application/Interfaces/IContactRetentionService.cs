using System.Threading;
using System.Threading.Tasks;

namespace MediaPortfolio.Application.Interfaces;

public interface IContactRetentionService
{
    Task DeleteOldReadSubmissionsAsync(CancellationToken cancellationToken = default);
}
