using Microsoft.Azure.WebJobs.Description;
using System;

namespace FtpBindings
{
    /// <summary>
    /// this attribute can only be used in method parameter
    /// </summary>
    /// <remarks>
    /// Azure function can return a value which is handled by trigger
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding(TriggerHandlesReturnValue = true)]
    public class FtpTriggerAttribute : GenericTriggerAttribute
    {        
        public FtpTriggerAttribute(string configKey)
        {
            this.ConfigKey = configKey;
        }
    }
}
