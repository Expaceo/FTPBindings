using System;

namespace FtpBindings
{
    public class GenericTriggerAttribute : Attribute
    {
        public string ConfigKey { get; set; } = String.Empty;
    }
}
