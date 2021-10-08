using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpBindings
{
    internal class GenericTriggerBinding<TOptions, TMessage, TResult, TListener> : ITriggerBinding
        where TOptions: IGenericOptions, new()
        where TMessage : class
        where TListener : GenericTriggerListener<TOptions, TMessage, TResult>
        where TResult : class
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _configKey;
        private readonly Dictionary<string, Type> _bindingDataContract;
        private readonly MemoryResponseHandler<TResult> _memoryResponseHandler;
        public GenericTriggerBinding(IConfiguration configuration, ILogger logger, string configKey)
        {
            _logger = logger;
            LogInformation("GenericTriggerBinding:ctor");
            _configuration = configuration;
            _configKey = configKey;
            _bindingDataContract = new Dictionary<string, Type>();
            _bindingDataContract.Add("$return", typeof(object).MakeByRefType());
            _memoryResponseHandler = new MemoryResponseHandler<TResult>();
        }

        public Type TriggerValueType => typeof(TMessage);

        public IReadOnlyDictionary<string, Type> BindingDataContract => _bindingDataContract;

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            LogInformation("BindAsync");

            var valueProvider = new ObjectValueProvider(value, this.TriggerValueType);
            Dictionary<string, object> aggregateBindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var result = new TriggerData(valueProvider, aggregateBindingData);
            result.ReturnValueProvider = _memoryResponseHandler;
            return Task.FromResult<ITriggerData>(result);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            LogInformation("CreateListenerAsync");
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            TOptions options = new TOptions();
            //available keys list for configuration
            //AzureFunctionsJobHost, AzureWebJobsConfigurationSection, AzureWebJobsScriptRoot
            var section = _configuration.GetSection($"AzureFunctionsJobHost:{_configKey}");
            section.Bind(options);

            var result = Activator.CreateInstance(typeof(TListener), context.Executor, context.Descriptor.Id, _logger, options, _memoryResponseHandler);
            return Task.FromResult((IListener)result);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new GenericTriggerParameterDescriptor();
        }

        internal class GenericTriggerParameterDescriptor : TriggerParameterDescriptor
        {
            #region Fields
            internal const string TRIGGER_NAME = "GenericTrigger";
            private const string TRIGGER_DESCRIPTION = "New changes at {0}";
            #endregion

            #region Methods
            public override string GetTriggerReason(IDictionary<string, string> arguments)
            {
                return String.Format(TRIGGER_DESCRIPTION, DateTime.UtcNow.ToString("o"));
            }
            #endregion
        }

        private void LogInformation(string message)
        {
            _logger.LogInformation($"{this.GetHashCode()}:{message}");
        }
    }
}