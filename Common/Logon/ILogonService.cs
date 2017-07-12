
namespace Common.Logon
{
    public interface ILogonService
    {
        void HandleSuccess(string identifier);

        void HandleFailure(string identifier);

        bool IsRejected(string identifier);
    }
}