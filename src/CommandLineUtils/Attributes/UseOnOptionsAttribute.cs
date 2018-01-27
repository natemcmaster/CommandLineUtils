using System;

namespace McMaster.Extensions.CommandLineUtils
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UseOnOptionsAttribute : Attribute
    {
    }
}
