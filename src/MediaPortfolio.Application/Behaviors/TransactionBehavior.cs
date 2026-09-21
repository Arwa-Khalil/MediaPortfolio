using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IApplicationDbContext dbContext, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Wrap commands in transaction (convention: Command in name)
        if (!typeof(TRequest).Name.EndsWith("Command"))
        {
            return await next();
        }

        try
        {
            var response = await next();
            await _dbContext.SaveChangesAsync(cancellationToken);
            return response;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error handling transaction for {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
