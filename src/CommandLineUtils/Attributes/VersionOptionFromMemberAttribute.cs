﻿// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The option used to determine if version info should be displayed.
    /// The value for the version information is provided by the properties or members specified.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VersionOptionFromMemberAttribute : OptionAttributeBase
    {
        /// <summary>
        /// Initializes an instance of <see cref="VersionOptionFromMemberAttribute"/> with <c>--version</c> as the template.
        /// </summary>
        public VersionOptionFromMemberAttribute()
            : this(Strings.DefaultVersionTemplate)
        { }

        /// <summary>
        /// Initializes an instance of <see cref="VersionOptionFromMemberAttribute"/> with <c>--version</c> as the template.
        /// </summary>
        /// <param name="template">The version template.</param>
        public VersionOptionFromMemberAttribute(string template)
        {
            Template = template;
        }

        /// <summary>
        /// The name of the property or method that returns short version information.
        /// </summary>
        public string? MemberName { get; set; }

        /// <summary>
        /// The option template. This is parsed into the short and long name.
        /// </summary>
        public new string Template { get; set; }

        internal CommandOption Configure(CommandLineApplication app, Type type, Func<object> targetInstanceFactory)
        {
            Func<string>? shortFormGetter = null;
            Func<string>? longFormGetter = null;

            if (MemberName != null)
            {
                var methods = ReflectionHelper.GetPropertyOrMethod(type, MemberName);

                if (methods.Length == 0)
                {
                    throw new InvalidOperationException(Strings.NoPropertyOrMethodFound(MemberName, type));
                }

                if (methods.Length > 1)
                {
                    throw new AmbiguousMatchException("Multiple properties or methods match the name " + MemberName);
                }

                shortFormGetter = () =>
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
                    return (string)methods[0].Invoke(targetInstanceFactory.Invoke(), Array.Empty<object>());
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                };
            }

            var option = app.VersionOption(Template, shortFormGetter, longFormGetter);
            Configure(option);
            return option;
        }
    }
}
