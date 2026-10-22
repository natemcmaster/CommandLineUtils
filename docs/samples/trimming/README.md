Trimming sample for McMaster.Extensions.CommandLineUtils
==========================================================

Demonstrates publishing a trimmed, self-contained, single-file app that uses
`McMaster.Extensions.CommandLineUtils.CommandLineApplication.Execute<TApp>`.

See [issue #539](https://github.com/natemcmaster/CommandLineUtils/issues/539).

Instructions
------------

From the repository root, publish the sample for your runtime (Linux x64 example):

```bash
dotnet publish docs/samples/trimming/TrimmingSample.csproj -c Release -r linux-x64
```

Because the project sets `EnableTrimAnalyzer` and `TreatWarningsAsErrors`, the publish
itself fails if the trimmer finds any unannotated reflection reachable from this sample -
so this project doubles as a regression check, not just a demo.

Run the produced binary (path is printed at the end of the publish, under
`docs/samples/trimming/bin/Release/net8.0/<rid>/publish`):

```bash
./docs/samples/trimming/bin/Release/net8.0/linux-x64/publish/TrimmingSample --subject Trimming
```

Expected output: `Hello, Trimming!`
