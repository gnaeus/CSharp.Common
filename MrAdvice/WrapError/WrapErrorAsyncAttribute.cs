using System;
using System.Reflection;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;
using Common.Api;
using Common.Exceptions;

namespace MrArvice.Aspects
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WrapErrorAsyncAttribute : Attribute, IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            MethodInfo methodInfo = (MethodInfo)context.TargetMethod;
            Type returnType = methodInfo.ReturnType.GetGenericArguments()[0];

            IApiStatus response;

            try
            {
                await context.ProceedAsync();
                return;
            }
            catch (ValidationException exception)
            {
                response = (IApiStatus)Activator.CreateInstance(returnType);
                response.ValidationErrors = exception.Errors;
            }
            catch (BusinessException exception)
            {
                response = (IApiStatus)Activator.CreateInstance(returnType);
                response.ErrorMessage = exception.Message;

                IApiError error = response as IApiError;
                if (error != null)
                {
                    error.ErrorCode = exception.Code;
                }
            }
            catch (Exception exception)
            {
                response = (IApiStatus)Activator.CreateInstance(returnType);
                response.ErrorMessage = exception.Message;
                
                Type exceptionType = exception.GetType();
                if (exceptionType.IsGenericType
                    && exceptionType.GetGenericTypeDefinition() == typeof(BusinessException<>))
                {
                    try
                    {
                        dynamic dynamicResponse = response;
                        dynamic dynamicException = exception;

                        dynamicResponse.ErrorCode = dynamicException.Code;
                    }
                    catch { }
                }
            }

            response.IsSuccess = false;
            context.ReturnValue = Task.FromResult(response);
        }
    }
}
