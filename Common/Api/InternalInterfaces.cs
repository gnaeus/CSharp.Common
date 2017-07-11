using Common.Validation;

namespace Common.Api
{
    internal interface IApiResponse
    {
        bool IsSuccess { set; }
        string ErrorMessage { set; }
        ValidationError[] ValidationErrors { set; }
    }

    internal interface IApiResult
    {
        object Data { set; }
    }
    
    internal interface IApiError
    {
        object ErrorCode { set; }
    }
}
