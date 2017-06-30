using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AspNet.WebApi.ActionResults
{
    public class FileStreamResult : IHttpActionResult
    {
        public string FileName { get; }
        public string ContentType { get; }
        private readonly Stream Stream;

        public FileStreamResult(Stream stream, string fileName, string contentType = null)
        {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }
            Stream = stream;
            FileName = fileName;
            ContentType = contentType ?? MimeMapping.GetMimeMapping(Path.GetExtension(fileName));
        }

        public FileStreamResult(string filePath, string contentType = null)
            : this(File.OpenRead(filePath), Path.GetFileName(filePath), contentType) { }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StreamContent(Stream)
            };

            var headers = response.Content.Headers;

            if (Stream.CanSeek) {
                headers.ContentLength = Stream.Length;
            }

            headers.ContentType = new MediaTypeHeaderValue(ContentType);

            if (FileName != null) {
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {
                    FileName = Uri.EscapeUriString(FileName)
                };
            }
            
            return Task.FromResult(response);
        }
    }
}
