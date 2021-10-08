namespace FtpBindings
{
    public interface IGenericOptions
    {
        int MaxRetryOnFailure { get; set; }
        int MaxRetryDelayOnFailureMs { get; set; }
    }
    public class GenericOptions : IGenericOptions
    {
        public int MaxRetryOnFailure { get; set; } = 3;
        public int MaxRetryDelayOnFailureMs { get; set; } = 1000;
    }
}
