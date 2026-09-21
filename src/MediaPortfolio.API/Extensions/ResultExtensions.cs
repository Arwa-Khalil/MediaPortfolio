using System.Collections.Generic;
using System.Linq;
using MediaPortfolio.Application.Common;
using MediaPortfolio.API.Models;

namespace MediaPortfolio.API.Extensions;

public static class ResultExtensions
{
    public static object ToApiResponse<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return ApiResponse<T>.Success(result.Value!);
        }
        else
        {
            return ApiResponse<object>.Failure(
                result.ErrorCode ?? "INTERNAL_SERVER_ERROR", 
                result.ErrorMessage ?? "An unexpected error occurred."
            );
        }
    }
}
