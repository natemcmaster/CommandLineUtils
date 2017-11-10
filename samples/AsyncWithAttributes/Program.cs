// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

[Command(Name = "simpleget", Description = "A very simple downloader")]
[HelpOption]
class Program
{
    static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

    [Argument(0, Description = "The url to download")]
    private string Url { get; }

    private async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        if (string.IsNullOrEmpty(Url))
        {
            app.ShowHelp();
            return 0;
        }

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
        {
            Console.Error.WriteLine($"Invalid url '{Url}'");
            return 1;
        }

        var client = new HttpClient();
        var result = await client.GetAsync(uri);
        var content = await result.Content.ReadAsStringAsync();

        Console.WriteLine(content);

        return result.IsSuccessStatusCode ? 0 : 1;
    }
}
