# Changelog

## [Unreleased]

New features:
  - Added more validation attributes.
     - Add the `[FileExists]` attribute
     - Add the `[FileOrDirectoryExists]` attribute
     - Add the `[DirectoryExists]` attribute
     - Add the `[LegalFilePath]` attribute
     - Add the `[AllowedValues]` attribute
  - Added a new, fluent API for validation.
     - Added `Option().Accepts()` and `Argument().Accepts()`
     - Add `.ExistingFile()`
     - Add `.ExistingFileOrDirectory()`
     - Add `.ExistingDirectory()`
     - Add `.EmailAddress()`
     - Add `.LegalFilePath()`
     - Add `.MinLength(length)`
     - Add `.MaxLength(length)`
     - Add `.RegularExpression(pattern)`
     - Add `.Values(string[] allowedValues)`

API improvements:
  - Support parsing enums
  - [@atruskie] - Support for parsing double and floats
  - [@rmcc13] - `HelpOption` can be set to be inherited by all subcommands
  - Add `VersionOptionFromMember` to use a property or method as the source of version information

## [v2.1.1]
**Dec. 27, 2017**

Bug fixes:
 - Do not show validation error messages when --help or --version are specified
 - Fix help text to show correct short option when `OptionAttribute.ShortName` is overriden

## [v2.1.0]
**Dec. 12, 2017**

New features:
 - Attributes. Simplify command line argument definitions by adding attributes to a class that represents options and arguments.
    - Options defined as `[Option]` or `[Argument]`, `[Subcommand]`.
    - Command parsing options can be defined with `[Command]` and `[Subcmomand]`.
    - Special options include `[HelpOption]` and `[VersionOption]`.
    - Validation. You can use `[Required]` and any other ValidationAttribute to validate input on options and arguments.
 - Async from end to end. Using C# 7.1 and attribute binding, your console app can be async from top to bottom.
 - Required options and arguments. Added `CommandOption.IsRequired()` and `CommandArgument.IsRequired()`.

New API
 - [@demosdemon] - added `Prompt.GetSecureString`
 - `Prompt.GetYesNo`, `Prompt.GetPassword`, and more. Added API for interactively getting responses on the console.
 - Added `OptionAttribute`, `ArgumentAttribute`, `CommandAttribute`, `SubcommandAttribute`, `HelpOptionAttribute`, and `VersionOptionAttribute`.
 - `CommandLineApplication.Execute<TApp>()` - executes an app where `TApp` uses attributes to define its options
 - `CommandLineApplication.ExecuteAsync<TApp>()` - sample thing, but async.
 - `CommandLineApplication.ResponseFileHandling` - the parser can treat arguments that begin with '@' as response files.
   Response files contain arguments that will be treated as if they were passed on command line.
 - [@couven92] - added overloads for  few new `CommandLineApplication.VersionOptionFromAssemblyAttributes` and `.VerboseOption` extension methods

Minor bug fixes:
 - Add return types to `.VerboseOption()` and ensure `.HasValue()` is true when HelpOption or VerboseOption are matched
 - Fix a NullReferenceException in some edge cases when parsing args
 - Fix bug where `DotNetExe.FullPath` might return the wrong location of the dotnet.exe file

Other:
 - [@kant2002] - added a new sample to demonstrate async usage

## [v2.0.1]
**Oct. 31, 2017**

 - [@couven92] - Add support for .NET Standard 1.6

## [v2.0.0]
**Sep. 16, 2017**

 - Initial version of this library.
 - Forked Microsoft.Extensions.CommandLineUtils
 - Renamed root namespace to McMaster.Extensions.CommandLineUtils
 - Added a handful of new API
 - Updated TFM to support .NET Standard 2.0


[@atruskie]: https://github.com/atruskie
[@couven92]: https://github.com/couven92
[@demosdemon]: https://github.com/demosdemon
[@kant2002]: https://github.com/kant2002
[@rmcc13]: https://github.com/rmcc13

[Unreleased]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.1.1...HEAD
[v2.1.1]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.1.0...v2.1.1
[v2.1.0]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.0.1...v2.1.0
[v2.0.1]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.0.0...v2.0.1
[v2.0.0]: https://github.com/natemcmaster/CommandLineUtils/compare/b0c662d331c35ccf3145875cdef850df7e896c0f...v2.0.0

