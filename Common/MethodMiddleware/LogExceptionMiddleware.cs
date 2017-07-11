using System;
using System.Reflection;
using NLog;
using Common.Exceptions;

namespace Common.MethodMiddleware
{
    public class LogExceptionMiddleware : IMethodMiddleware
    {
        readonly ILogger Logger;

        public LogExceptionMiddleware(ILogger logger)
        {
            Logger = logger;
        }

        public object Invoke(MethodInfo methodInfo, object arguments, Func<object> method)
        {
            string caller = $"{methodInfo.ReflectedType.Name}.{methodInfo.Name}";

            try
            {
                return method.Invoke();
            }
            catch (BusinessException exception)
            {
                Logger.Warn(exception, $"[{caller}]");
                throw;
            }
            catch (Exception exception)
            {
                if (exception is IBusinessException)
                {
                    Logger.Warn(exception, $"[{caller}]");
                }
                else
                {
                    Logger.Error(exception, $"[{caller}]");
                }
                throw;
            }
        }
    }
}
