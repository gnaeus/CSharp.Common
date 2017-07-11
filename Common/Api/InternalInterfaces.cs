using Common.Validation;

namespace Common.Api
{
    internal interface IApiStatus
    {
        bool IsSuccess { set; }
        string ErrorMessage { set; }
        ValidationError[] ValidationErrors { set; }
    }

    internal interface IApiResult<TResult>
    {
        TResult Data { set; }
    }

    internal interface IApiError
    {
        string ErrorCode { set; }
        string ErrorMessage { set; }
    }

    internal interface IApiError<TError>
        where TError : struct
    {
        TError? ErrorCode { set; }
        string ErrorMessage { set; }
    }
}
