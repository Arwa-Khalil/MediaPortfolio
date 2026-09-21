namespace MediaPortfolio.Application.DTOs;

public record MediaUploadUrlResultDto(string UploadUrl, string CloudflareStreamId);
public record MediaImageUploadUrlResultDto(string UploadUrl, string CloudflareImageId);
