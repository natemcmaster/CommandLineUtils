CommandLineUtils 2.x
====================

**CommandLineUtils** is a library which helps developers implement command line applications in .NET.
The primary goal of the library is to assist with parsing command line arguments and executing the correct
commands related to those arguments. The library also provides various other utilities such as input helpers.

<div class="row">
    <div class="col-md-4">
        <div class="panel panel-default" style="min-height: 140px">
            <div class="panel-body">
                <p><strong><a href="docs/intro.md">Documentation</a></strong></p>
                <p>Tutorials to create your first .NET command line application, and docs on how to use the library.</p>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="panel panel-default" style="min-height: 140px">
            <div class="panel-body">
                <p><strong> [2.x API Reference](xref:latest_2_x_api_ref) </strong></p>
                <p>Read the API documentation for this library.</p>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="panel panel-default" style="min-height: 140px">
            <div class="panel-body">
                <p><strong><a href="https://github.com/natemcmaster/CommandLineUtils/tree/main/docs/samples/">Samples</a></strong></p>
                <p>View sample projects which use CommandLineUtils.</p>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="panel panel-default" style="min-height: 140px">
            <div class="panel-body">
                <p><strong><a href="https://github.com/natemcmaster/CommandLineUtils">Source Code and Issue Tracker</a></strong></p>
                <p>The project is open-source on GitHub.</p>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="panel panel-default" style="min-height: 140px">
            <div class="panel-body">
                <p><strong><a href="https://nuget.org/packages/McMaster.Extensions.CommandLineUtils">NuGet</a></strong></p>
                <p>See the latest releases of this library as a NuGet package.</p>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="panel panel-default" style="min-height: 140px">
            <div class="panel-body">
                <p><strong><a href="https://github.com/natemcmaster/CommandLineUtils/tree/main/CHANGELOG.md">Version history</a></strong></p>
                <p>Read notes about fixes and enhancements per release.</p>
            </div>
        </div>
    </div>
</div>


Using this library, you can write a command line application without doing the heavy lifting to support automated help text generation,
masking input for passwords, parsing argument syntax, validation, etc.

```c#
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
