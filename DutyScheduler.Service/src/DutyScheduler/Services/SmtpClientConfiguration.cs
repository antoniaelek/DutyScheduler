namespace DutyScheduler.Services
{
    public class SmtpClientConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string MailboxName { get; set; }
        public string MailboxAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
