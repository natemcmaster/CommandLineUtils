// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Helper methods for <see cref="IConsole"/>.
    /// </summary>
    public static class ConsoleExtensions
    {

        //
        // WriteLine extensions
        //

        /// <summary>
        /// Writes an empty line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <returns>the console.</returns>
        public static IConsole WriteLine(this IConsole console)
        {
            console.Out.WriteLine();
            return console;
        }

        /// <summary>
        /// Writes a string followed by a line terminator.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>the console.</returns>
        public static IConsole WriteLine(this IConsole console, string value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg">Argument used to format.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, string format, params object[] arg)
        {
            console.Out.WriteLine(format, arg);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first argument to replace in the format string.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, string format, object arg0)
        {
            console.Out.WriteLine(format, arg0);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first argument to replace in the format string.</param>
        /// <param name="arg1">The second argument to replace in the format string.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, string format, object arg0, object arg1)
        {
            console.Out.WriteLine(format, arg0, arg1);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first argument to replace in the format string.</param>
        /// <param name="arg1">The second argument to replace in the format string.</param>
        /// <param name="arg2">The third argument to replace in the format string.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, string format, object arg0, object arg1, object arg2)
        {
            console.Out.WriteLine(format, arg0, arg1, arg2);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, ulong value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, bool value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, char value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes an array of characters as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, char[] buffer)
        {
            console.Out.WriteLine(buffer);
            return console;
        }

        /// <summary>
        /// Formats and writes a portion of a character buffer as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="index">The start index.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, char[] buffer, int index, int count)
        {
            console.Out.WriteLine(buffer, index, count);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, decimal value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, double value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, uint value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, int value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, object value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, float value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value as a new line.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole WriteLine(this IConsole console, long value)
        {
            console.Out.WriteLine(value);
            return console;
        }

        //
        // Write methods
        //

        /// <summary>
        /// Writes a string console output.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>the console.</returns>
        public static IConsole Write(this IConsole console, string value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg">Argument used to format.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, string format, params object[] arg)
        {
            console.Out.Write(format, arg);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first argument to replace in the format string.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, string format, object arg0)
        {
            console.Out.Write(format, arg0);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first argument to replace in the format string.</param>
        /// <param name="arg1">The second argument to replace in the format string.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, string format, object arg0, object arg1)
        {
            console.Out.Write(format, arg0, arg1);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first argument to replace in the format string.</param>
        /// <param name="arg1">The second argument to replace in the format string.</param>
        /// <param name="arg2">The third argument to replace in the format string.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, string format, object arg0, object arg1, object arg2)
        {
            console.Out.Write(format, arg0, arg1, arg2);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, uint value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, decimal value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, int value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, ulong value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, bool value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, char value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes an array of characters.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, char[] buffer)
        {
            console.Out.Write(buffer);
            return console;
        }

        /// <summary>
        /// Formats and writes a portion of a character buffer.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="index">The start index.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, char[] buffer, int index, int count)
        {
            console.Out.Write(buffer, index, count);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, double value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, long value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, object value)
        {
            console.Out.Write(value);
            return console;
        }

        /// <summary>
        /// Formats and writes a value.
        /// </summary>
        /// <param name="console">The console.</param>
        /// <param name="value">The value.</param>
        /// <returns>The console.</returns>
        public static IConsole Write(this IConsole console, float value)
        {
            console.Out.Write(value);
            return console;
        }
    }
}
