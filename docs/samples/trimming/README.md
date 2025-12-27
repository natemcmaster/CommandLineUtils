Trimming sample for McMaster.Extensions.CommandLineUtils

This sample demonstrates publishing a trimmed, single-file app that uses McMaster.Extensions.CommandLineUtils.

Instructions

1. From the repository root, publish the sample (macOS Apple Silicon example):

```bash
dotnet publish docs/samples/trimming/TrimmingSample.csproj -c Release -r osx-arm64
```

2. Inspect the build output. Trimming warnings will be emitted during publish and can help identify missing annotations.

3. Run the produced single-file binary (path will be under `docs/samples/trimming/bin/Release/net8.0/osx-arm64/publish`).

Expected behavior

- Without trimming annotations in the library, published binary may throw exceptions like "No method named 'OnExecute' or 'OnExecuteAsync' could be found." at runtime because reflection targets were removed by the trimmer.
- After adding [DynamicallyAccessedMembers] annotations to key generic entry points and constructor-creating paths (see issue #539), the binary should run and print `MyCommand executed`.

Notes

This sample references the local project to ensure you're testing the current source tree. For library consumers, annotations should be present on NuGet packages distributed by the library.

References
- https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming
- https://github.com/natemcmaster/CommandLineUtils/issues/539
