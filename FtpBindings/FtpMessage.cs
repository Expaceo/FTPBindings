namespace FtpBindings
{
    public class FtpMessage
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = new byte[0];
    }
}
