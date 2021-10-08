using Microsoft.Azure.WebJobs;

namespace FtpBindings
{
    internal static class FtpTriggerWebJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddFtpTrigger(this IWebJobsBuilder builder)
        {
            builder.AddExtension<FtpExtensionConfig>();
            return builder;
        }
    }
}
