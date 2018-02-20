using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <inheritdoc />
    /// <summary>
    /// Mark this property to use if passed into a GetOption prompt.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UseOnOptionsAttribute : Attribute
    {
    }
}
