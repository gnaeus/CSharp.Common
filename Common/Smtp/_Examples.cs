using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using Common.Smtp;

partial class _Examples
{
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
}