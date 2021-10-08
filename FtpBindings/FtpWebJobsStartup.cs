using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(FtpBindings.FtpWebJobsStartup))]

namespace FtpBindings
{
    /// <summary>
    /// When user reference this assembly, automatically configure his IWebJobsBuilder
    /// </summary>
    internal class FtpWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddFtpTrigger();
        }
    }
}
