using System;

namespace Common.Validation
{
    public struct ValidationError
    {
        public string PropertyPath { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ValidationError(string path, string code, string message)
        {
            PropertyPath = path;
            ErrorCode = code;
            ErrorMessage = message;
        }

        public static readonly ValidationError[] EmptyErrors = new ValidationError[0];
    }
}
