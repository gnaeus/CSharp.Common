### SmtpConnectionSettings

```cs
public class SmtpConnectionSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; }
}
```

### SmtpConnectionChecker
Utility for checking availability of SMTP Server.

```cs
public class SmtpConnectionChecker
{
    public SmtpConnectionChecker(ISmtpConnectionSettings settings);

    public virtual bool ServerIsReady();
}

### SmtpMailSender
Utility for sending mail messages and handling errors.

```cs
public class SmtpMailSender
{
    public SmtpMailSender(SmtpConnectionSettings settings, NLog.ILogger logger);

    public virtual async Task<bool> TrySend(MailMessage message);
    public virtual async Task<bool> TrySend(byte[] serializedMessage);
}
```

Example:
```cs
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using Common.Smtp;

class MailService
{
    readonly SmtpConnectionChecker _checker;
    readonly SmtpMailSender _sender;

    public async Task SendMailMessage(MailMessage message)
    {
        while (!_checker.ServerIsReady())
        {
            await Task.Delay(1000);
        }

        bool success = await _sender.TrySend(message);

        Debug.WriteIf(!success, "MailMessage sending failed");
    }
}
```
