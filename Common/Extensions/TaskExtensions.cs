using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class TaskExtensions
    {
        public static T AsSyncronous<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void AsSyncronous(this Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
