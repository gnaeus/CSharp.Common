using Common.Validation;

namespace Common.Api
{
    public class ApiStatus : IApiStatus, IApiError
    {
        public bool IsSuccess { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;
    }
    
    public class ApiStatus<TError> : IApiStatus, IApiError<TError>
        where TError : struct
    {
        public bool IsSuccess { get; set; }
        public TError? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;
    }
}
