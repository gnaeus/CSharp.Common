using System;
using Common.Validation;

namespace Common.Exceptions
{
    /// <summary>
    /// Exception for passing validation errors.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationError[] Errors { get; } = ValidationError.EmptyErrors;

        public ValidationException(string path, string code, string message)
        {
            Errors = new[] { new ValidationError(path, code, message) };
        }

        public ValidationException(params ValidationError[] errors)
        {
            Errors = errors;
        }
    }
}
