using System.Net.Mail;
using RazorEngine.Common.Mail;

partial class _Examples
{
    class EmailModel
    {
        public string UserName { get; set; }
        public string PasswordLink { get; set; }
    }

    class EmailService
    {
        readonly MailTemplateEngine _mailTemplateEngine;

        public MailMessage CreateEmail(EmailModel model)
        {
            MailMessage message = _mailTemplateEngine.CreateMessage(
                from: "site@example.com",
                to: "user@example.com",
                templatePath: "~/Views/Email/ResetPassword.cshtml",
                model: model,
                fromName: "My awesome site",
                isBodyHtml: true);

            Attachment attachment = _mailTemplateEngine.CreateAttachment(
                templatePath: "~/Views/Email/Attachments/ResetPassword.cshtml",
                model: model);

            attachment.ContentId = "password-links";

            message.Attachments.Add(attachment);

            return message;
        }
    }
}
