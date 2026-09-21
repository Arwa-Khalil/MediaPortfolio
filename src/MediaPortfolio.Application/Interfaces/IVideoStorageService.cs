using System.Threading.Tasks;

namespace MediaPortfolio.Application.Interfaces;

public interface IVideoStorageService
{
    Task<MediaUploadUrlResult> GetUploadUrlAsync();
    Task DeleteVideoAsync(string cloudflareStreamId);
}
