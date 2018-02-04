// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
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
        public string MemberName { get; set; }

        internal CommandOption Configure(CommandLineApplication app, Type type, object targetInstance)
        {
            Func<string> shortFormGetter = null;
            Func<string> longFormGetter = null;

            if (MemberName != null)
            {
                var methods = GetPropertyOrMethod(type, MemberName);

                if (methods.Length == 0)
                {
                    throw new InvalidOperationException($"Could not find a property or method named {MemberName} on type {type.FullName}");
                }

                if (methods.Length > 1)
                {
                    throw new AmbiguousMatchException("Multiple properties or methods match the name " + MemberName);
                }

                shortFormGetter = () =>
                {
                    return methods[0].Invoke(targetInstance, Constants.EmptyArray) as string;
                };
            }

            return app.VersionOption(Template, shortFormGetter, longFormGetter);
        }

        private static MethodInfo[] GetPropertyOrMethod(Type type, string name)
        {
            const BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            return type.GetTypeInfo()
                .GetMethods(binding)
                .Where(m => m.Name == name)
                .Concat(type.GetTypeInfo().GetProperties(binding).Where(m => m.Name == name).Select(p => p.GetMethod))
                .Where(m => m.ReturnType == typeof(string) && m.GetParameters().Length == 0)
                .ToArray();
        }
    }
}
