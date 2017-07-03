## BusinessException
Exception with error code and message that passed to end user of application.

```cs
public class BusinessException : Exception
{
    public string Code { get; set; }

    public BusinessException(string code, string message);
}

public class BusinessException<TError> : Exception
    where TError : struct
{
    public TError Code { get; set; }

    public BusinessException(TError code, string message);
}
```

## ValidationException
Exception for passing validation errors.

```cs
public class ValidationException : Exception
{
    public ValidationError[] Errors { get; }

    public ValidationException(string path, string code, string message);
    public ValidationException(params ValidationError[] errors);
}
```
