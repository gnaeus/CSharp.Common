using Common.Api.Tags;
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

        public static implicit operator ApiStatus(bool isSuccess)
        {
            return new ApiStatus
            {
                IsSuccess = isSuccess,
            };
        }

        public static implicit operator ApiStatus(SuccessTag tag)
        {
            return new ApiStatus
            {
                IsSuccess = true,
            };
        }

        public static implicit operator ApiStatus(ErrorTag<string> tag)
        {
            return new ApiStatus
            {
                IsSuccess = false,
                ErrorCode = tag.ErrorCode,
                ErrorMessage = tag.ErrorMessage,
            };
        }
    }

    /// <summary>
    /// Structure for passing status of service operation with possible validation and logic errors.
    /// </summary>
    public class ApiStatus<TError> : IApiStatus, IApiError<TError>
        where TError : struct
    {
        public bool IsSuccess { get; set; }
        public TError? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;

        public static implicit operator ApiStatus<TError>(bool isSuccess)
        {
            return new ApiStatus<TError>
            {
                IsSuccess = isSuccess,
            };
        }

        public static implicit operator ApiStatus<TError>(SuccessTag tag)
        {
            return new ApiStatus<TError>
            {
                IsSuccess = true,
            };
        }

        public static implicit operator ApiStatus<TError>(ErrorTag<TError> tag)
        {
            return new ApiStatus<TError>
            {
                IsSuccess = false,
                ErrorCode = tag.ErrorCode,
                ErrorMessage = tag.ErrorMessage,
            };
        }
    }
}
