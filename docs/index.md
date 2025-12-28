CommandLineUtils
================

**CommandLineUtils** is a library which helps developers implement command line applications in .NET.
The primary goal of the library is to assist with parsing command line arguments and executing the correct
commands related to those arguments. The library also provides various other utilities such as input helpers.

<div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
    <div class="col">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title"><a href="docs/intro.md">Documentation</a></h5>
                <p class="card-text">Tutorials to create your first .NET command line application, and docs on how to use the library.</p>
            </div>
        </div>
    </div>
    <div class="col">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title"><a href="api/index.md">API Reference</a></h5>
                <p class="card-text">Read the API documentation for this library.</p>
            </div>
        </div>
    </div>
    <div class="col">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title"><a href="https://github.com/natemcmaster/CommandLineUtils/tree/main/docs/samples/">Samples</a></h5>
                <p class="card-text">View sample projects which use CommandLineUtils.</p>
            </div>
        </div>
    </div>
    <div class="col">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title"><a href="https://github.com/natemcmaster/CommandLineUtils">Source Code</a></h5>
                <p class="card-text">The project is open-source on GitHub.</p>
            </div>
        </div>
    </div>
    <div class="col">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title"><a href="https://nuget.org/packages/McMaster.Extensions.CommandLineUtils">NuGet</a></h5>
                <p class="card-text">See the latest releases of this library as a NuGet package.</p>
            </div>
        </div>
    </div>
    <div class="col">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title"><a href="https://github.com/natemcmaster/CommandLineUtils/blob/main/CHANGELOG.md">Changelog</a></h5>
                <p class="card-text">Read notes about fixes and enhancements per release.</p>
            </div>
        </div>
    </div>
</div>

---

## Quick Example

Using this library, you can write a command line application without doing the heavy lifting to support automated help text generation,
masking input for passwords, parsing argument syntax, validation, etc.

```c#
using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;

class Program
{
    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    [Option("-n")]
    [Range(0, 10)]
    [Required]
    public int Count { get; }

    public void OnExecute()
    {
        for (var i = 0; i < Count; i ++)
        {
            Prompt.GetPassword("Enter your password: ");
        }
    }
}
```
