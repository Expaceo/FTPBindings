using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FtpBindings
{
    /// <summary>
    /// this class hold the result of azure function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MemoryResponseHandler<T> : IValueBinder, IValueProvider
        where T : class
    {
        public Type Type => typeof(T).MakeByRefType();

        public T FonctionResult { get; private set; } = default(T);

        public Task<object> GetValueAsync()
        {
            //called by listener
            return Task.FromResult((object)FonctionResult);
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            //called by Azure function framework
            FonctionResult = (T)value;
            return Task.CompletedTask;
        }

        public string ToInvokeString()
        {
            return "response";
        }
    }
}
