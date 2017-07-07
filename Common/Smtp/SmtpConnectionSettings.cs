namespace Common.Smtp
{
    public class SmtpConnectionSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }
}
