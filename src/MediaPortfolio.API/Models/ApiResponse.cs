using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaPortfolio.API.Models;

/// <summary>
/// The single, canonical HTTP response envelope for every endpoint in the API.
/// Matches §1.4 of API-Contract-v1.0.docx exactly.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }

    /// <summary>UTC timestamp of when the response was produced.</summary>
    public string Timestamp { get; init; } = DateTime.UtcNow.ToString("o");

    /// <summary>Populated on success; null on failure.</summary>
    public T? Data { get; init; }

    /// <summary>Present only on paginated list endpoints; null otherwise.</summary>
    public PaginationMeta? Meta { get; init; }

    /// <summary>Populated on failure; null on success.</summary>
    public IReadOnlyList<ApiError>? Errors { get; init; }

    // -----------------------------------------------------------------
    // Factory helpers
    // -----------------------------------------------------------------

    public static ApiResponse<T> Success(T data, PaginationMeta? meta = null) =>
        new()
        {
            IsSuccess = true,
            Data = data,
            Meta = meta,
            Errors = null
        };

    public static ApiResponse<T> Failure(string code, string message, string? field = null) =>
        new()
        {
            IsSuccess = false,
            Data = default,
            Errors = new[] { new ApiError(code, field, message) }
        };

    public static ApiResponse<T> Failure(IReadOnlyList<ApiError> errors) =>
        new()
        {
            IsSuccess = false,
            Data = default,
            Errors = errors
        };
}

/// <summary>One entry in the errors array (§1.4 failure envelope).</summary>
public sealed record ApiError(string Code, string? Field, string Message);

/// <summary>Pagination metadata returned in the meta field for list endpoints (§1.8).</summary>
public sealed record PaginationMeta(int Page, int PageSize, int TotalCount);

