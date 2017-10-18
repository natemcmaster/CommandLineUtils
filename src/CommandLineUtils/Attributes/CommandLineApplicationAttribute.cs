using System;

namespace McMaster.Extensions.CommandLineUtils
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class CommandLineApplicationAttribute : Attribute
    {
        public CommandLineApplicationAttribute()
        { }

        public bool ThrowOnUnexpectedArgs { get; set; }
    }
}
