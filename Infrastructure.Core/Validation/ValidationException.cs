namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation
{
    public class ValidationError
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ValidationException : Exception
    {
        public IEnumerable<ValidationError> Errors { get; set; }

        public ValidationException(string message, IEnumerable<ValidationError> errors = null)
        : base(message)
        {
            Errors = errors;
        }
    }

    public static class ValidationErrorExtensions
    {
        public static ValidationException ToValidationException(this FluentValidation.Results.ValidationResult validationResult)
        {
            return new ValidationException("One or more validation errors occurred",
                validationResult.Errors.Select(e => new ValidationError
                {
                    PropertyName = e.PropertyName,
                    ErrorMessage = e.ErrorMessage
                }));
        }
    }
}
