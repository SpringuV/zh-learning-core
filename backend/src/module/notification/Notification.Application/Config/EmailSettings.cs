namespace Notification.Application.Config
{
    // the SMTP server & email account details readable at runtime
    public class MailSettings
    {
        // the section name in appsettings.json for binding configuration
        public const string SectionName = "MailSettings";
        public string EmailId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; } = 0;
        public bool UseSSL { get; set; } = false;
    }
}
