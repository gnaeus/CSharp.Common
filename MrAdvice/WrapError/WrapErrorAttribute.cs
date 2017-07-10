using System;
using System.Reflection;
using ArxOne.MrAdvice.Advice;
using Common.Api;
using Common.Exceptions;

namespace MrArvice.Aspects
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WrapErrorAttribute : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            MethodInfo methodInfo = (MethodInfo)context.TargetMethod;
            Type returnType = methodInfo.ReturnType;

            IApiStatus response;

            try
            {
                context.Proceed();
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

                IApiError error = response as IApiError;
                if (error != null)
                {
                    error.ErrorCode = exception.Code;
                    error.ErrorMessage = exception.Message;
                }
            }
            catch (Exception exception)
            {
                response = (IApiStatus)Activator.CreateInstance(returnType);

                Type exceptionType = exception.GetType();
                if (exceptionType.IsGenericType
                    && exceptionType.GetGenericTypeDefinition() == typeof(BusinessException<>))
                {
                    try
                    {
                        dynamic dynamicResponse = response;
                        dynamic dynamicException = exception;

                        dynamicResponse.ErrorCode = dynamicException.Code;
                        dynamicResponse.ErrorMessage = exception.Message;
                    }
                    catch { }
                }
            }

            response.IsSuccess = false;
            context.ReturnValue = response;
        }
    }
}
