using System;
using System.IO;
using System.Net.Mail;
using RazorEngine.Templating;

namespace RazorEngine.Common.Mail
{
    /// <summary>
    /// Utility for creating <see cref="MailMessage"/> from Razor view.
    /// </summary>
    public class MailTemplateEngine
    {
        public MailMessage CreateMessage(
            string from,
            string to,
            string subject,
            string body,
            string fromName = null)
        {
            MailAddress fromAddress = fromName != null
                ? new MailAddress(from, fromName)
                : new MailAddress(from);

            return new MailMessage(fromAddress, new MailAddress(to)) {
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8,
                Subject = subject,
                Body = body,
            };
        }

        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public MailMessage CreateMessage(
            string from,
            string to,
            string templatePath,
            object model,
            string fromName = null,
            bool isBodyHtml = false)
        {
            Type modelType = GetModelType(model);
            CompileAndCacheTemplate(templatePath, modelType);
            
            var viewBag = new DynamicViewBag();
            dynamic dynamicViewBag = viewBag;

            string body = Engine.Razor.Run(templatePath, modelType, model, viewBag);
            string subject = dynamicViewBag.Subject;
            
            MailMessage msg = CreateMessage(from, to, subject, body, fromName);
            msg.IsBodyHtml = isBodyHtml;
            return msg;
        }
        
        public Attachment CreateAttachment(
            string templatePath,
            object model,
            string mediaType = "text/plain")
        {
            Type modelType = GetModelType(model);
            CompileAndCacheTemplate(templatePath, modelType);

            var viewBag = new DynamicViewBag();
            dynamic dynamicViewBag = viewBag;

            string content = Engine.Razor.Run(templatePath, modelType, model, viewBag);
            
            var attachment = Attachment.CreateAttachmentFromString(content, mediaType);

            attachment.ContentDisposition.FileName = dynamicViewBag.FileName;

            return attachment;
        }

        private static Type GetModelType(object model)
        {
            if (model == null) {
                return null;
            }

            Type modelType = model.GetType();

            return modelType.IsVisible ? modelType : null;
        }

        private static void CompileAndCacheTemplate(string templatePath, Type modelType)
        {
            if (!Engine.Razor.IsTemplateCached(templatePath, modelType)) {
                string realPath = MapPath(templatePath);
                
                string template = File.ReadAllText(realPath);

                Engine.Razor.Compile(template, templatePath, modelType);
            }
        }

        private static string MapPath(string path)
        {
            if (path == null) {
                throw new ArgumentNullException("path", "TemplatePath is not resolved");
            }
            if (path.StartsWith("~/")) {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(baseDir, path.Substring(2));
            }
            if (!File.Exists(path)) {
                throw new InvalidOperationException($"TemplatePath {path} is not resolved");
            }
            return path;
        }
    }
}