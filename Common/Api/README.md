## ApiWrapper
Utility for wrapping operation results and logging exceptions.

```cs
public class ApiWrapper : IApiWrapper
{
    public ApiWrapper(NLog.ILogger logger);

    public ApiStatus Execute(Action method);
    public ApiStatus<TError> Execute<TError>(Action method);
    public ApiResult<TResult> Execute<TResult>(Func<TResult> method);
    public ApiResult<TResult, TError> Execute<TResult, TError>(Func<TResult> method);
    public async Task<ApiStatus> ExecuteAsync(Func<Task> method);
    public async Task<ApiStatus<TError>> ExecuteAsync<TError>(Func<Task> method);
    public async Task<ApiResult<TResult>> ExecuteAsync<TResult>(Func<Task<TResult>> method);
    public async Task<ApiResult<TResult, TError>> ExecuteAsync<TResult, TError>(Func<Task<TResult>> method);
}
```

Example:
```cs
using System.Data.SqlClient;
using Common.Api;
using Common.Exceptions;
using static Common.Api.ApiHelper;

class Model { }

enum ErrorCodes { GeneralError }

class WebService
{
    readonly ApiWrapper _apiWrapper;
    readonly ApplicationService _applicationService;

    public ApiResult<Model, ErrorCodes> DoSomething(Model argument)
    {
        return _apiWrapper.Execute<Model, ErrorCodes>(() =>
        {
            return _applicationService.DoSomething(argument);
        });
    }

    public ApiResult<Model, ErrorCodes> DoSomethingElse(Model argument)
    {
        if (argument == null)
        {
            return Error(ErrorCodes.GeneralError, $"Argument {nameof(argument)} is required");
        }
        return Ok(new Model());
    }
}

class ApplicationService
{
    public Model DoSomething(Model argument)
    {
        if (argument == null)
        {
            throw new ValidationException(
                nameof(argument), "Required", $"Argument {nameof(argument)} is required");
        }
        try
        {
            // do something
            return argument;
        }
        catch (SqlException)
        {
            throw new BusinessException<ErrorCodes>(
                ErrorCodes.GeneralError, "Something went wrong, please try again");
        }
    }
}
```

## ApiResult
Structure for passing result of service operation with possible validation and logic errors.

```cs
public class ApiResult<TResult>
{
    public bool IsSuccess { get; set; }
    public virtual TResult Data { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; }
}

public class ApiResult<TResult, TError>
        where TError : struct
{
    public bool IsSuccess { get; set; }
    public virtual TResult Data { get; set; }
    public TError? ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; }
}
```

## ApiStatus
Structure for passing status of service operation with possible validation and logic errors.

```cs
public class ApiStatus : IApiStatus, IApiError
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; }
}

public class ApiStatus<TError> : IApiStatus, IApiError<TError>
    where TError : struct
{
    public bool IsSuccess { get; set; }
    public TError? ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; };
}
```

## ApiHelper
Static helper for wrapping operation results and errors to common structures

__`Ok()`__  
Utility for returning result from method

__`Ok<TResult>(TResult data)`__  
Utility for returning result from method

__`Error<TError>(TError code, string message = null)`__  
Utility for returning error from method
