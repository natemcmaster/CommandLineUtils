// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace McMaster.Extensions.CommandLineUtils.SourceGeneration
{
    /// <summary>
    /// Execute handler that throws an error when invoked.
    /// Used for error cases like ambiguous methods.
    /// </summary>
    internal sealed class ErrorExecuteHandler : IExecuteHandler
    {
        private readonly string _errorMessage;

        public ErrorExecuteHandler(string errorMessage)
        {
            _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        }

        public bool IsAsync => false;

        public Task<int> InvokeAsync(object model, CommandLineApplication app, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(_errorMessage);
        }
    }
}
