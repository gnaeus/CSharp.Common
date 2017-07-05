
namespace Common.Api.Tags
{
    public struct ErrorTag { }

    public struct ErrorTag<TError>
    {
        internal readonly TError ErrorCode;
        internal readonly string ErrorMessage;

        internal ErrorTag(TError code, string message)
        {
            ErrorCode = code;
            ErrorMessage = message;
        }
    }
}
