using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Threading.Tasks;

namespace FtpBindings
{
    internal class ObjectValueProvider : IValueProvider
    {
        private readonly object _value;
        private readonly Type _valueType;
        public Type Type => _valueType;

        public ObjectValueProvider(object value, Type valueType)
        {
            if (value != null && !valueType.IsInstanceOfType(value))
            {
                throw new InvalidOperationException("value is not of the correct type.");
            }
            _valueType = valueType;
            _value = value;
        }

        public Task<object> GetValueAsync()
        {
            return Task.FromResult(_value);
        }

        public string ToInvokeString()
        {
            return _value?.ToString();
        }
    }
}
