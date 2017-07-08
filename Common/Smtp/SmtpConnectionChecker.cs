using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace Common.Smtp
{
    /// <summary>
    /// Utility for checking availability of SMTP Server.
    /// </summary>
    public class SmtpConnectionChecker
    {
        readonly SmtpConnectionSettings _settings;

        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        public SmtpConnectionChecker(SmtpConnectionSettings settings)
        {
            if (String.IsNullOrEmpty(settings.Server))
            {
                throw new ArgumentException("Server can't be empty", nameof(settings));
            }
            if (settings.Port <= 0 || settings.Port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(settings), "Port should be between 0 and 65536 ");
            }
            _settings = settings;
        }
        
        /// <remarks>
        /// http://stackoverflow.com/questions/1633391/testing-smtp-server-is-running-via-c-sharp/1633419#1633419
        /// </remarks>
        public virtual bool ServerIsReady()
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(_settings.Server, _settings.Port);

                    using (Stream stream = client.GetStream())
                    using (SslStream sslStream = _settings.EnableSsl ? new SslStream(stream) : null)
                    {
                        if (sslStream != null)
                        {
                            sslStream.AuthenticateAsClient(_settings.Server);
                        }
                        using (var writer = new StreamWriter(sslStream ?? stream))
                        using (var reader = new StreamReader(sslStream ?? stream))
                        {
                            writer.WriteLine("EHLO " + _settings.Server);
                            writer.Flush();

                            string response = reader.ReadLine();
                            return response != null && response.StartsWith("220 ");
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
