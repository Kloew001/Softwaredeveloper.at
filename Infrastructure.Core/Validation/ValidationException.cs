namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation;

public class ValidationError
{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }
}

public class ValidationException : Exception
{
    public IEnumerable<ValidationError> ValidationErrors { get; set; }

    public ValidationException(string message, IEnumerable<ValidationError> errors = null)
    : base(message)
    {
        ValidationErrors = errors;
    }
}
