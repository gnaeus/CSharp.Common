### FileStreamResult
`IHttpActionResult` for passing streams as files to client (streams are not materialized to RAM).

```cs
public class FileStreamResult : IHttpActionResult
{
    public string FileName { get; }
    public string ContentType { get; }

    public FileStreamResult(Stream stream, string fileName, string contentType = null);
    public FileStreamResult(string filePath, string contentType = null);
}
```

Example:
```cs
class FileController : ApiController
{
    public IHttpActionResult DownloadFile(string filePath)
    {
        return new FileStreamResult(filePath, "application/json");
    }

    public IHttpActionResult DownloadMemoryStream()
    {
        // stream will be disposed later by AspNet Web API
        var ms = new MemoryStream();

        using (StreamWriter writer = new StreamWriter(ms, Encoding.UTF8))
        {
            writer.Write("test test test...");
        }

        ms.Seek(0, SeekOrigin.Begin);

        return new FileStreamResult(ms, "MemoryStream.txt", "text/plain");
    }
}
```

### ApiExceptionLogger
NLog global exception logger for AspNet Web API.

```cs
public class ApiExceptionLogger : ExceptionLogger
{
    public ApiExceptionLogger(NLog.ILogger logger);
}
```
