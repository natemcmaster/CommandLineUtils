// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

[Command(Name = "simplecurl", Description = "A very simple http client")]
[HelpOption("-?")]
class Program
{
    static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

    [Argument(0, Description = "The url to download")]
    private string Url { get; }

    [Option("-X|--request", Description = "HTTP Method: GET or POST. Defaults to GET.")]
    public HttpMethod RequestMethod { get; } = HttpMethod.Get;

    [Option(Description = "Data to attach to the POST body.")]
    public string Data { get; }

    /// <summary>
    /// Property types of ValueTuple{bool,T} translate to CommandOptionType.SingleOrNoValue.
    /// Input            | Value
    /// -----------------|--------------------------------
    /// (none)           | (false, default(TraceLevel))
    /// --trace          | (true, TraceLevel.Normal)
    /// --trace:normal   | (true, TraceLevel.Normal)
    /// --trace:verbose  | (true, TraceLevel.Verbose)
    /// </summary>
    [Option]
    public (bool HasValue, TraceLevel level) Trace { get; }

    private HttpClient _client;

    private async Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Url))
        {
            app.ShowHelp();
            return 0;
        }

        _client = new HttpClient();

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
        {
            Console.Error.WriteLine($"Invalid url '{Url}'");
            return 1;
        }

        HttpResponseMessage result;
        LogTrace(TraceLevel.Verbose, $"Starting {RequestMethod} request to {uri}");
        switch (RequestMethod)
        {
            case HttpMethod.Get:
                result = await GetAsync(uri, cancellationToken);
                break;
            case HttpMethod.Post:
                result = await PostAsync(uri, cancellationToken);
                break;
            default:
                throw new NotImplementedException();
        }
        LogTrace(TraceLevel.Info, $"HTTP {result.Version} {(int)result.StatusCode} {result.ReasonPhrase}");
        return result.IsSuccessStatusCode ? 0 : 1;
    }

    private void LogTrace(TraceLevel level, string message)
    {
        if (!Trace.HasValue) return;
        if (Trace.level >= level)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"{level}: {message}");
            Console.ResetColor();
        }
    }

    private async Task<HttpResponseMessage> PostAsync(Uri uri, CancellationToken cancellationToken)
    {
        var content = new ByteArrayContent(Encoding.ASCII.GetBytes(Data ?? string.Empty));
        return await _client.PostAsync(uri, content, cancellationToken);
    }

    private async Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancellationToken)
    {
        var result = await _client.GetAsync(uri, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var content = await result.Content.ReadAsStringAsync();

        Console.WriteLine(content);

        return result;
    }
}

public enum HttpMethod
{
    Get,
    Post,
}

public enum TraceLevel
{
    Info = 0,
    Verbose,
}
