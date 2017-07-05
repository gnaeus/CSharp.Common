
namespace Common.Api.Tags
{
    public struct SuccessTag { }

    public struct SuccessTag<TResult>
    {
        internal readonly TResult Data;

        internal SuccessTag(TResult data)
        {
            Data = data;
        }
    }
}
