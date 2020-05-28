using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.Hosting.CommandLine.Internal;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Extensions methods for the <see cref="HostBuilderContext"/>
    /// </summary>
    public static class HostBuilderContextExtensions
    {
        /// <summary>
        /// Get the <see cref="CommandLineContext"/> used to run the app
        /// </summary>
        /// <param name="context">This instance</param>
        /// <returns>The <see cref="CommandLineContext"/> used to run the app</returns>
        public static CommandLineContext GetCommandLineContext(this HostBuilderContext context)
            => (CommandLineContext)context.Properties[typeof(CommandLineState)];
    }
}
