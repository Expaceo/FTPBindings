namespace FtpBindings
{
    public class ConnectionOptions : GenericOptions
    {
        public string Host { get; set; } = string.Empty;
        public bool secureFTP { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string InFilter { get; set; } = string.Empty;
        public string InFolder { get; set; } = string.Empty;
        public string OutFolder { get; set; } = string.Empty;

        public string ErrFolder { get; set; } = string.Empty;
    }
}
