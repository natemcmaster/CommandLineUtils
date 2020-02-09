// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Process access to a console pager, which supports scrolling and search.
    /// <para>
    /// This is done by piping into `less` command (Linux/macOS only.)
    /// Windows is currently not supported.
    /// </para>
    /// </summary>
    public class Pager : IDisposable
    {
        private readonly Lazy<Process?> _less;
        private readonly TextWriter _fallbackWriter;
        private bool _enabled;
        private bool _disposed;
        private string _prompt = "Use arrow keys to scroll\\. Press 'q' to exit\\.";

        /// <summary>
        /// Initializes a new instance of <see cref="Pager" /> which displays output in a console pager.
        /// </summary>
        public Pager()
            : this(PhysicalConsole.Singleton)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="Pager" /> which displays output in a console pager.
        /// </summary>
        /// <param name="console">The console to write to.</param>
        public Pager(IConsole console)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

#if NET45
            // if .NET Framework, assume we're on Windows unless it's running on Mono.
            _enabled = Type.GetType("Mono.Runtime") != null;
#elif NETSTANDARD2_0
            _enabled = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !console.IsOutputRedirected;
#else
#error Update target frameworks
#endif

            _less = new Lazy<Process?>(CreateWriter);
            _fallbackWriter = console.Out;
        }

        /// <summary>
        /// The prompt to display at the bottom of the pager.
        /// <seealso href="https://www.computerhope.com/unix/uless.htm#Prompts" /> for details.
        /// </summary>
        public string Prompt
        {
            get => _prompt;
            set
            {
                if (_less.IsValueCreated)
                {
                    throw new InvalidOperationException("Cannot set the prompt on the pager after the pager has begun");
                }

                _prompt = value;
            }
        }

        /// <summary>
        /// <para>
        /// Gets an object which can be used to write text into the pager.
        /// </para>
        /// <para>
        /// This fallsback to <see cref="IConsole.Out" /> if the pager is not available.
        /// </para>
        /// </summary>
        public TextWriter Writer
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(Pager));
                }

                return _less.Value?.StandardInput ?? _fallbackWriter;
            }
        }

        /// <summary>
        /// This will wait until the user exits the pager.
        /// </summary>
        public void WaitForExit() => Dispose();

        /// <summary>
        /// Force close the pager.
        /// </summary>
        public void Kill()
        {
            if (_less.IsValueCreated && _less.Value != null)
            {
                _less.Value.Kill();
            }
        }

        private Process? CreateWriter()
        {
            if (!_enabled)
            {
                return null;
            }

            var args = new List<string>
            {
                "-K", // enables CTRL+C to exit
                $"--prompt={Prompt}"
            };

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "less",
                    Arguments = ArgumentEscaper.EscapeAndConcatenate(args),
                    RedirectStandardInput = true,
#if NET45
                    UseShellExecute = false,
#endif
                }
            };

            try
            {
                process.Start();
                return process;
            }
            catch (Exception ex)
            {
                if (DotNetCliContext.IsGlobalVerbose())
                {
                    Console.Error.WriteLine("debug: Failed to start pager: " + ex.ToString());
                }
                _enabled = false;
                return null;
            }
        }

        /// <summary>
        /// This will wait until the user exits the pager.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (!_less.IsValueCreated)
            {
                return;
            }

            var process = _less.Value;
            if (process == null)
            {
                return;
            }

            process.StandardInput.Dispose();
            process.WaitForExit();
            process.Dispose();
        }
    }
}
