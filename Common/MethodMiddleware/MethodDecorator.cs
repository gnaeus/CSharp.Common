using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Common.MethodMiddleware
{
    /// <summary>
    /// Utility applying middleware to specified methods.
    /// </summary>
    public partial class MethodDecorator
    {
        readonly ICollection<IMethodMiddleware> _methodMiddlewares = new List<IMethodMiddleware>();
        readonly ICollection<IMethodAsyncMiddleware> _asyncMiddlewares = new List<IMethodAsyncMiddleware>();

        public MethodDecorator Use(params IMiddleware[] middlewares)
        {
            foreach (IMiddleware middleware in middlewares)
            {
                var methodMiddleware = middleware as IMethodMiddleware;
                if (methodMiddleware != null)
                {
                    _methodMiddlewares.Add(methodMiddleware);
                }

                var asyncMiddleware = middleware as IMethodAsyncMiddleware;
                if (asyncMiddleware != null)
                {
                    _asyncMiddlewares.Add(asyncMiddleware);
                }
            }

            return this;
        }

        object Invoke(
            object arguments, Func<object> method,
            params IMethodMiddleware[] middlewares)
        {
            MethodInfo methodInfo = (MethodInfo)new StackFrame(2).GetMethod();

            foreach (IMethodMiddleware middleware in middlewares.Reverse())
            {
                Func<object> capturedMetod = method;
                method = () => middleware.Invoke(methodInfo, arguments, capturedMetod);
            }

            foreach (IMethodMiddleware middleware in _methodMiddlewares.Reverse())
            {
                Func<object> capturedMetod = method;
                method = () => middleware.Invoke(methodInfo, arguments, capturedMetod);
            }

            return method.Invoke();
        }
        
        Task<object> Invoke(
            object arguments, Func<Task<object>> method,
            params IMethodAsyncMiddleware[] middlewares)
        {
            MethodInfo methodInfo = (MethodInfo)new StackFrame(2).GetMethod();

            foreach (IMethodAsyncMiddleware middleware in middlewares.Reverse())
            {
                Func<Task<object>> capturedMetod = method;
                method = () => middleware.Invoke(methodInfo, arguments, capturedMetod);
            }

            foreach (IMethodAsyncMiddleware middleware in _asyncMiddlewares.Reverse())
            {
                Func<Task<object>> capturedMetod = method;
                method = () => middleware.Invoke(methodInfo, arguments, capturedMetod);
            }

            return method.Invoke();
        }

        Func<object> MakeFunc(Action action)
        {
            return () =>
            {
                action.Invoke();
                return null;
            };
        }

        Func<Task<object>> MakeFunc(Func<Task> action)
        {
            return async () =>
            {
                await action.Invoke();
                return null;
            };
        }
    }
}
