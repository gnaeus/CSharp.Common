using System.IO;
using System.Text;
using System.Web.Http;
using AspNet.WebApi.Common.ActionResults;

partial class _Examples
{
    #region ActionResults

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

    #endregion
}
