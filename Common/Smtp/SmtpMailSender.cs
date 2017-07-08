using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using NLog;
using Common.Mail;

namespace Common.Smtp
{
    /// <summary>
    /// Utility for sending mail messages and handling errors.
    /// </summary>
    public class SmtpMailSender
    {
        readonly SmtpConnectionSettings _settings;
        readonly ILogger _logger;
        
        public SmtpMailSender(SmtpConnectionSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        /// <exception cref="SmtpException" />
        public virtual async Task<bool> TrySend(MailMessage message)
        {
            using (var client = new SmtpClient(_settings.Server, _settings.Port))
            using (message)
            {
                client.Credentials = new NetworkCredential(_settings.Login, _settings.Password);
                client.EnableSsl = _settings.EnableSsl;

                try
                {
                    await client.SendMailAsync(message);

                    _logger.Info("Email was sent");
                    return true;
                }
                catch (SmtpFailedRecipientsException ex)
                {
                    _logger.Warn(ex, "Email has missing recipients");
                    return true;
                }
                catch (SmtpException ex)
                {
                    _logger.Error(ex, "Email causes error with StatusCode = " + ex.StatusCode);
                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Warn(ex, "Email was not sent");
                    return false;
                }
            }
        }

        public virtual async Task<bool> TrySend(byte[] serializedMessage)
        {
            MailMessage message;
            try
            {
                message = MailMessageBinarySerializer.Deserialize(serializedMessage);

                List<MailValidationError> errors = message.ValidateAddresses().ToList();
                if (errors.Any())
                {
                    _logger.Error("Email has invalid format " + String.Join(", ", errors));
                    return false;
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Email has invalid binary format");
                return false;
            }

            return await TrySend(message);
        }
    }
}
