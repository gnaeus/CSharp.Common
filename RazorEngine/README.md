### MailTemplateEngine
Utility for creating `System.Net.Mail.MailMessage` from Razor view.

```cs
public class MailTemplateEngine
{
    public MailMessage CreateMessage(
        string from,
        string to,
        string templatePath,
        object model,
        string fromName = null,
        bool isBodyHtml = false);

    public Attachment CreateAttachment(
        string templatePath,
        object model,
        string mediaType = "text/plain");
}
```

Example:

```cs
using System.Net.Mail;
using RazorEngine.Common.Mail;

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
```

~/Views/Email/ResetPassword.cshtml
```html
@model EmailModel
@{ 
    ViewBag.Subject = "Please reset your password";
}
Dear @Model.UserName, please <a href="@Model.PasswordLink">reset</a> your password.
```

~/Views/Email/Attachments/ResetPassword.cshtml
```html
@model EmailModel
@{
    ViewBag.FileName = "reset_password_link_" + Model.UserName + ".txt";
}
@Model.PasswordLink
```
