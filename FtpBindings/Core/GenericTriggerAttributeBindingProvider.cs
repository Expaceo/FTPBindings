using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FtpBindings
{
    internal class GenericTriggerAttributeBindingProvider<TTriggerAttribute, TOptions, TMessage, TResult, TListener> : ITriggerBindingProvider
        where TTriggerAttribute : GenericTriggerAttribute
        where TOptions : IGenericOptions, new()
        where TMessage : class
        where TResult : class
        where TListener : GenericTriggerListener<TOptions, TMessage, TResult>
    {       
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public GenericTriggerAttributeBindingProvider(IConfiguration configuration,
            INameResolver nameResolver, ILoggerFactory loggerFactory, string triggerLogCategory)
        {
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory(triggerLogCategory));
            _configuration = configuration;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //extract info from attribute
            ParameterInfo parameter = context.Parameter;
            TTriggerAttribute triggerAttribute = parameter.GetCustomAttribute<TTriggerAttribute>(inherit: false);
            if (triggerAttribute is null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }
            //check type of trigger parameter
            CheckParameterType(parameter.ParameterType);
            //create trigger binding
            var triggerBindingInstance = (ITriggerBinding)new GenericTriggerBinding<TOptions, TMessage, TResult, TListener>(_configuration, _logger, triggerAttribute.ConfigKey);

            return Task.FromResult<ITriggerBinding>(triggerBindingInstance);
        }        

        protected virtual void CheckParameterType(Type parameterType)
        {
            if (!typeof(TMessage).Equals(parameterType))
            {
                throw new InvalidOperationException($"Only '{typeof(TMessage).FullName}' is supported type.");
            }
        }
    }
}
