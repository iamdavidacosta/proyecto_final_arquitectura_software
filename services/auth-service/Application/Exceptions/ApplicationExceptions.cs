namespace AuthService.Application.Exceptions;

public class ApplicationException : Exception
{
    public string Code { get; }
    
    public ApplicationException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException(IDictionary<string, string[]> errors) 
        : base("VALIDATION_ERROR", "One or more validation errors occurred")
    {
        Errors = errors;
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string entity, object key) 
        : base("NOT_FOUND", $"{entity} with key '{key}' was not found")
    {
    }
}

public class ConflictException : ApplicationException
{
    public ConflictException(string message) 
        : base("CONFLICT", message)
    {
    }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Unauthorized") 
        : base("UNAUTHORIZED", message)
    {
    }
}
