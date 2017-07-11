using Common.Api.Tags;
using Common.Validation;

namespace Common.Api
{
    /// <summary>
    /// Structure for passing result of service operation with possible validation and logic errors.
    /// </summary>
    public class ApiResult<TResult> : IApiResponse, IApiError, IApiResult, IApiResult<TResult>
    {
        public bool IsSuccess { get; set; }
        public virtual TResult Data { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;

        object IApiResult.Data
        {
            set
            {
                if (value is TResult)
                {
                    Data = (TResult)value;
                }
            }
        }

        object IApiError.ErrorCode
        {
            set
            {
                if (value is string)
                {
                    ErrorCode = (string)value;
                }
            }
        }

        public static implicit operator ApiResult<TResult>(TResult data)
        {
            return new ApiResult<TResult>
            {
                IsSuccess = true,
                Data = data,
            };
        }

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
    public class ApiResult<TResult, TError> : IApiResponse, IApiError, IApiError<TError>, IApiResult, IApiResult<TResult>
        where TError : struct
    {
        public bool IsSuccess { get; set; }
        public virtual TResult Data { get; set; }
        public TError? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationError[] ValidationErrors { get; set; } = ValidationError.EmptyErrors;

        object IApiResult.Data
        {
            set
            {
                if (value is TResult)
                {
                    Data = (TResult)value;
                }
            }
        }

        object IApiError.ErrorCode
        {
            set
            {
                if (value is TError)
                {
                    ErrorCode = (TError)value;
                }
            }
        }

        public static implicit operator ApiResult<TResult, TError>(TResult data)
        {
            return new ApiResult<TResult, TError>
            {
                IsSuccess = true,
                Data = data,
            };
        }

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
