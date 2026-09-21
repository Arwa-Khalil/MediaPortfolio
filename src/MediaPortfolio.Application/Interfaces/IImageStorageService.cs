using System.Threading.Tasks;

namespace MediaPortfolio.Application.Interfaces;

public interface IImageStorageService
{
    Task<MediaUploadUrlResult> GetUploadUrlAsync();
    Task DeleteImageAsync(string cloudflareImageId);
}
