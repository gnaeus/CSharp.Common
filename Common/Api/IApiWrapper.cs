using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Common.Api
{
    public interface IApiWrapper
    {
        ApiStatus Execute(Action method, [CallerMemberName] string caller = null);

        ApiStatus<TError> Execute<TError>(
            Action method, [CallerMemberName] string caller = null)
            where TError : struct;

        ApiResult<TResult> Execute<TResult>(
            Func<TResult> method, [CallerMemberName] string caller = null);

        ApiResult<TResult, TError> Execute<TResult, TError>(
            Func<TResult> method, [CallerMemberName] string caller = null)
            where TError : struct;

        Task<ApiStatus> ExecuteAsync(
            Func<Task> method, [CallerMemberName] string caller = null);

        Task<ApiStatus<TError>> ExecuteAsync<TError>(
            Func<Task> method, [CallerMemberName] string caller = null)
            where TError : struct;
        Task<ApiResult<TResult>> ExecuteAsync<TResult>(
            Func<Task<TResult>> method, [CallerMemberName] string caller = null);

        Task<ApiResult<TResult, TError>> ExecuteAsync<TResult, TError>(
            Func<Task<TResult>> method, [CallerMemberName] string caller = null)
            where TError : struct;
    }
}
