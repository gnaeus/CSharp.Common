using Common.Api.Tags;
using Common.Validation;

namespace Common.Api
{
    /// <summary>
    /// Structure for passing result of service operation with possible validation and logic errors.
    /// </summary>
    public class ApiResult<TResult> : IApiStatus, IApiError, IApiResult<TResult>
    {
        public bool IsSuccess { get; set; }
        public virtual TResult Data { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;

        public static implicit operator ApiResult<TResult>(SuccessTag<TResult> successTag)
        {
            return new ApiResult<TResult>
            {
                IsSuccess = true,
                Data = successTag.Data,
            };
        }

        public static implicit operator ApiResult<TResult>(ErrorTag<string> tag)
        {
            return new ApiResult<TResult>
            {
                IsSuccess = false,
                ErrorCode = tag.ErrorCode,
                ErrorMessage = tag.ErrorMessage,
            };
        }
    }

    /// <summary>
    /// Structure for passing result of service operation with possible validation and logic errors.
    /// </summary>
    public class ApiResult<TResult, TError> : IApiStatus, IApiError<TError>, IApiResult<TResult>
        where TError : struct
    {
        public bool IsSuccess { get; set; }
        public virtual TResult Data { get; set; }
        public TError? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;

        public static implicit operator ApiResult<TResult, TError>(SuccessTag<TResult> successTag)
        {
            return new ApiResult<TResult, TError>
            {
                IsSuccess = true,
                Data = successTag.Data,
            };
        }

        public static implicit operator ApiResult<TResult, TError>(ErrorTag<TError> tag)
        {
            return new ApiResult<TResult, TError>
            {
                IsSuccess = false,
                ErrorCode = tag.ErrorCode,
                ErrorMessage = tag.ErrorMessage,
            };
        }
    }
}
