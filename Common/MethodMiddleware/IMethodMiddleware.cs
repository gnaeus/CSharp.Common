using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Common.MethodMiddleware
{
    public interface IMiddleware { }

    public interface IMethodMiddleware : IMiddleware
    {
        object Invoke(MethodInfo methodInfo, object arguments, Func<object> method);
    }

    public interface IMethodAsyncMiddleware : IMiddleware
    {
        Task<object> Invoke(MethodInfo methodInfo, object arguments, Func<Task<object>> method);
    }
}
