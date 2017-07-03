using Common.Validation;

namespace Common.Api
{
    /// <summary>
    /// Structure for passing status of service operation with possible validation and logic errors.
    /// </summary>
    public class ApiStatus : IApiStatus, IApiError
    {
        public bool IsSuccess { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;
    }

    /// <summary>
    /// Structure for passing status of service operation with possible validation and logic errors.
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    public class ApiStatus<TError> : IApiStatus, IApiError<TError>
        where TError : struct
    {
        public bool IsSuccess { get; set; }
        public TError? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;
    }
}
