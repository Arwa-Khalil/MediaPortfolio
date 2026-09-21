using System;
using System.Threading.Tasks;

namespace MediaPortfolio.Application.Interfaces;

public interface IAnalyticsCounter
{
    Task IncrementVideoViewAsync(Guid videoId);
    Task IncrementWhatsAppClickAsync(Guid videoId);
    Task IncrementFormSubmissionAsync(Guid videoId);
}
