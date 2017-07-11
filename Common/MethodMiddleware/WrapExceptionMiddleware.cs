using System;
using System.Reflection;
using Common.Api;
using Common.Exceptions;

namespace Common.MethodMiddleware
{
    public class WrapExceptionMiddleware : IMethodMiddleware
    {
        public object Invoke(MethodInfo methodInfo, object arguments, Func<object> method)
        {
            Type returnType = methodInfo.ReturnType;

            IApiResponse response = (IApiResponse)Activator.CreateInstance(returnType);

            try
            {
                IApiResult result = response as IApiResult;
                if (result != null)
                {
                    result.Data = method.Invoke();
                }
                else
                {
                    method.Invoke();
                }
                
                response.IsSuccess = true;
                return response;
            }
            catch (ValidationException exception)
            {
                response.ValidationErrors = exception.Errors;
            }
            catch (BusinessException exception)
            {
                response.ErrorMessage = exception.Message;

                IApiError error = response as IApiError;
                if (error != null)
                {
                    error.ErrorCode = exception.Code;
                }
            }
            catch (Exception exception)
            {
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
            return response;
        }
    }
}
