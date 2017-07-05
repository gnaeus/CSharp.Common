using Common.Api.Tags;

namespace Common.Api
{
    public static class ApiHelper
    {
        public static SuccessTag Ok()
        {
            return new SuccessTag();
        }

        public static SuccessTag<TResult> Ok<TResult>(TResult data)
        {
            return new SuccessTag<TResult>(data);
        }
        
        public static ErrorTag<TError> Error<TError>(TError code, string message = null)
        {
            return new ErrorTag<TError>(code, message);
        }
    }
}
