using Common.Validation;

namespace Common.Api
{
    public class ApiResult<TResult> : IApiStatus, IApiError, IApiResult<TResult>
    {
        public bool IsSuccess { get; set; }
        public virtual TResult Data { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;
    }

    public class ApiResult<TResult, TError> : IApiStatus, IApiError<TError>, IApiResult<TResult>
        where TError : struct
    {
        public bool IsSuccess { get; set; }
        public virtual TResult Data { get; set; }
        public TError? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;
    }
}
