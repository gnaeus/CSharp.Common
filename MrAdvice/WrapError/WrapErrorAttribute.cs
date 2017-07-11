using System;
using System.Reflection;
using ArxOne.MrAdvice.Advice;
using ArxOne.MrAdvice.Annotation;
using Common.Api;
using Common.Exceptions;

namespace MrArvice.Aspects
{
    [Priority(Int32.MaxValue)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WrapErrorAttribute : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            MethodInfo methodInfo = (MethodInfo)context.TargetMethod;
            Type returnType = methodInfo.ReturnType;

            IApiResponse response;

            try
            {
                context.Proceed();
                return;
            }
            catch (ValidationException exception)
            {
                response = (IApiResponse)Activator.CreateInstance(returnType);
                response.ValidationErrors = exception.Errors;
            }
            catch (BusinessException exception)
            {
                response = (IApiResponse)Activator.CreateInstance(returnType);
                response.ErrorMessage = exception.Message;

                IApiError error = response as IApiError;
                if (error != null)
                {
                    error.ErrorCode = exception.Code;
                }
            }
            catch (Exception exception)
            {
                response = (IApiResponse)Activator.CreateInstance(returnType);
                response.ErrorMessage = exception.Message;

                IBusinessException businessException = exception as IBusinessException;
                if (businessException != null)
                {
                    IApiError error = response as IApiError;
                    if (error != null)
                    {
                        error.ErrorCode = businessException.Code;
                    }
                }
            }

            response.IsSuccess = false;
            context.ReturnValue = response;
        }
    }
}
