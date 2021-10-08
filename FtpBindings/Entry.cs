using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace FtpBindings
{
    [Extension("ftp", configurationSection: "ftp")]
    internal class FtpExtensionConfig : IExtensionConfigProvider
    {
        readonly IConfiguration _configuration;
        readonly ILoggerFactory _loggerFactory;
        readonly INameResolver _nameResolver;
        internal const string TRIGGERLOGCATEGORY = "Ftp";
        public FtpExtensionConfig(
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            INameResolver nameResolver)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            _nameResolver = nameResolver;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            //use generic provider, with our specific Type dedicated to FTP
            var triggerBindingProvider = new GenericTriggerAttributeBindingProvider<
                FtpTriggerAttribute,
                ConnectionOptions,
                FtpMessage,
                FtpActionResult,
                FtpTriggerListener>(_configuration, _nameResolver, _loggerFactory, TRIGGERLOGCATEGORY);
            //add link between FtpTriggerAttribute <--> our bindingProvider 
            context.AddBindingRule<FtpTriggerAttribute>()
                .BindToTrigger(triggerBindingProvider);
        }
    }
}
