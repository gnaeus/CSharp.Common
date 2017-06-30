using System;
using System.Runtime.CompilerServices;
using NLog;
using Common.Exceptions;

namespace Common.Api
{
    public class ApiWrapper
    {
        private readonly ILogger Logger;

        public ApiWrapper(ILogger logger)
        {
            Logger = logger;
        }

        private struct StubType { }

        public ApiStatus Execute(
            Action method,
            [CallerMemberName] string caller = null)
        {
            var response = new ApiStatus();
            Execute<StubType, StubType>(response, method, caller);
            return response;
        }

        public ApiStatus<TError> Execute<TError>(
            Action method,
            [CallerMemberName] string caller = null
        )
            where TError : struct
        {
            var response = new ApiStatus<TError>();
            Execute<StubType, TError>(response, method, caller);
            return response;
        }

        public ApiResult<TResult> Execute<TResult>(
            Func<TResult> method,
            [CallerMemberName] string caller = null)
        {
            var response = new ApiResult<TResult>();
            Execute<TResult, StubType>(response, method, caller);
            return response;
        }

        public ApiResult<TResult, TError> Execute<TResult, TError>(
            Func<TResult> method,
            [CallerMemberName] string caller = null
        )
            where TError : struct
        {
            var response = new ApiResult<TResult, TError>();
            Execute<TResult, TError>(response, method, caller);
            return response;
        }

        private void Execute<TResult, TError>(
            IApiStatus response,
            Delegate method,
            string caller
        )
            where TError : struct
        {
            try
            {
                IApiResult<TResult> result = response as IApiResult<TResult>;
                if (result != null)
                {
                    result.Data = ((Func<TResult>)method).Invoke();
                }
                else
                {
                    ((Action)method).Invoke();
                }
                response.IsSuccess = true;
            }
            catch (ValidationException exception)
            {
                response.ValidationErrors = exception.Errors;
                response.IsSuccess = false;
            }
            catch (BusinessException exception)
            {
                Logger.Warn(exception, $"[{caller}]");
                IApiError error = response as IApiError;
                if (error != null)
                {
                    error.ErrorCode = exception.Code;
                    error.ErrorMessage = exception.Message;
                }
                response.IsSuccess = false;
            }
            catch (BusinessException<TError> exception)
            {
                Logger.Warn(exception, $"[{caller}]");
                IApiError<TError> error = response as IApiError<TError>;
                if (error != null)
                {
                    error.ErrorCode = exception.Code;
                    error.ErrorMessage = exception.Message;
                }
                response.IsSuccess = false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"[{caller}]");
                response.IsSuccess = false;
            }
        }
    }
}
