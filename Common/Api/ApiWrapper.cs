using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NLog;
using Common.Exceptions;

namespace Common.Api
{
    /// <summary>
    /// Utility for wrapping operation results and logging exceptions.
    /// </summary>
    public class ApiWrapper : IApiWrapper
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

        public async Task<ApiStatus> ExecuteAsync(
            Func<Task> method,
            [CallerMemberName] string caller = null)
        {
            var response = new ApiStatus();
            await ExecuteAsync<StubType, StubType>(response, method, caller);
            return response;
        }

        public async Task<ApiStatus<TError>> ExecuteAsync<TError>(
            Func<Task> method,
            [CallerMemberName] string caller = null
        )
            where TError : struct
        {
            var response = new ApiStatus<TError>();
            await ExecuteAsync<StubType, TError>(response, method, caller);
            return response;
        }

        public async Task<ApiResult<TResult>> ExecuteAsync<TResult>(
            Func<Task<TResult>> method,
            [CallerMemberName] string caller = null)
        {
            var response = new ApiResult<TResult>();
            await ExecuteAsync<TResult, StubType>(response, method, caller);
            return response;
        }

        public async Task<ApiResult<TResult, TError>> ExecuteAsync<TResult, TError>(
            Func<Task<TResult>> method,
            [CallerMemberName] string caller = null
        )
            where TError : struct
        {
            var response = new ApiResult<TResult, TError>();
            await ExecuteAsync<TResult, TError>(response, method, caller);
            return response;
        }

        private async Task ExecuteAsync<TResult, TError>(
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
                    result.Data = await ((Func<Task<TResult>>)method).Invoke();
                }
                else
                {
                    await ((Func<Task>)method).Invoke();
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
