using System;

namespace MediaPortfolio.Domain.Exceptions;

public class NotFoundException : Exception
{
    public string Code { get; }

    public NotFoundException(string message) : base(message)
    {
        Code = "RESOURCE_NOT_FOUND";
    }
}
