namespace SoftwaredeveloperDotAt.Infrastructure.Core.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message)
        : base(message)
        { }
    }
}
