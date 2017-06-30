using System;
using Common.Validation;

namespace Common.Exceptions
{
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
