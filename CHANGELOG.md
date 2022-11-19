# Changelog

[Unreleased changes](https://github.com/natemcmaster/CommandLineUtils/compare/v4.0.2...HEAD):

## [v4.0.2](https://github.com/natemcmaster/CommandLineUtils/compare/v4.0.1...v4.0.2)

### Fixes
* @ernstc: bugfix: added check on the model type before it proceeds accessing the property ([#519])

[#519]: https://github.com/natemcmaster/CommandLineUtils/pull/519

## [v4.0.1](https://github.com/natemcmaster/CommandLineUtils/compare/v4.0.0...v4.0.1)

### Fixes

* [@xoofx]: use explicit constructors on validation attributes to workaround .NET 6 runtime trimming error ([#491])
* [@jakubqwe]: support the Description field of VersionOptionFromMemberAttribute ([#490])

[#490]: https://github.com/natemcmaster/CommandLineUtils/pull/490
[#491]: https://github.com/natemcmaster/CommandLineUtils/pull/491

## [v4.0.0](https://github.com/natemcmaster/CommandLineUtils/compare/v3.1.0...v4.0.0)

### Features
* [@scott-xu] and [@natemcmaster]: feature: add API for setting default value on options and arguments, and display them in help text ([#389] and [#420])
* [@natemcmaster]: cleanup: make .Values read-only on CommandArgument/Option ([#406])
* [@natemcmaster]: Make options, arguments, and commands read-only collections on CommandLineApplication ([#407])

### Fixes
* [@natemcmaster]: fix: pass the generic argument version in callback on .Option() and Argument() ([#405])
* [@natemcmaster]: add .NET Standard 2.1 and fix nullable type references ([#424] and [#425])
* [@natemcmaster]: fix new CI to correctly publish symbols to nuget.org
* [@scott-xu]:  show option types in help text when OptionAttribute.Template is set ([#429])
* [@skirchner989]:  change to not throw when a validator is not of type AttributeValidator ([#431])
* [@natemcmaster]: don't mask OperationCanceledException triggered by SIGINT ([#483])

[#429]: https://github.com/natemcmaster/CommandLineUtils/pull/429
[#431]: https://github.com/natemcmaster/CommandLineUtils/pull/431
[#484]: https://github.com/natemcmaster/CommandLineUtils/pull/484


### Other
* [@natemcmaster]: change from Azure DevOps to GitHub workflows

[#389]: https://github.com/natemcmaster/CommandLineUtils/pull/389
[#405]: https://github.com/natemcmaster/CommandLineUtils/pull/405
[#406]: https://github.com/natemcmaster/CommandLineUtils/pull/406
[#407]: https://github.com/natemcmaster/CommandLineUtils/pull/407
[#420]: https://github.com/natemcmaster/CommandLineUtils/pull/420
[#424]: https://github.com/natemcmaster/CommandLineUtils/pull/424
[#425]: https://github.com/natemcmaster/CommandLineUtils/pull/425

## [v3.1.0](https://github.com/natemcmaster/CommandLineUtils/compare/v3.0.0...v3.1.0)

### Improvements

* [@scott-xu]: Show allowed values in help text ([#369])
* [@scott-xu]: Generic Hosting: add overload of RunCommandLineApplicationAsync to support configuring of CommandLineApplication<T> ([#363])
* [@danielmeza]: implement API on host builder to fetch CommandLineContext from non DI contexts ([#368])
* [@hellfirehd] and [@cbcrouse]: feature: add API to allow calling UseCommandLineApplication&lt;TApp&gt; prior to Run*Async() ([#409] and [#415])

### Bug fixes
* [@natemcmaster]: ensure clustering options is detected correctly when subcommands added via AddSubcommand ([#393])
* [@scott-xu]: make UnrecognizedArgumentHandling per command scope ([#371])
* [@scott-xu]: show allowed values in help text correctly when multiple `AllowedValuesAttribute` are used ([#375])
* [@scott-xu]: show allowed values in help text for Nullable/ValueTuple<bool,T> option/argument ([#390])
* [@scott-xu]: update help text to show allowed values from `AllowedValues` attribute instead of Enum values ([#391])

### Docs
* [@chaami]: Fix Readme.md on Prompt.GetYesNo ([#402])
* [@cuperman007] docs: added using statements to code sample in index.md ([#384])
* [@scott-xu]: Fix broken links in docs ([#377])
* [@scott-xu]: cleanup: add a period to end sentences for built-in descriptions ([#364])

### Other

* [@natemcmaster]: Change default branch to 'main' ([#373])
* [@natemcmaster]: Stop code signing binaries and packages ([#379])
* [@natemcmaster]: add dotnet-format to CI, and other code/project file cleanups


[#363]: https://github.com/natemcmaster/CommandLineUtils/pull/363
[#364]: https://github.com/natemcmaster/CommandLineUtils/pull/364
[#367]: https://github.com/natemcmaster/CommandLineUtils/pull/367
[#368]: https://github.com/natemcmaster/CommandLineUtils/pull/368
[#369]: https://github.com/natemcmaster/CommandLineUtils/pull/369
[#371]: https://github.com/natemcmaster/CommandLineUtils/pull/371
[#373]: https://github.com/natemcmaster/CommandLineUtils/pull/373
[#375]: https://github.com/natemcmaster/CommandLineUtils/pull/375
[#377]: https://github.com/natemcmaster/CommandLineUtils/pull/377
[#379]: https://github.com/natemcmaster/CommandLineUtils/pull/379
[#384]: https://github.com/natemcmaster/CommandLineUtils/pull/384
[#390]: https://github.com/natemcmaster/CommandLineUtils/pull/390
[#391]: https://github.com/natemcmaster/CommandLineUtils/pull/391
[#393]: https://github.com/natemcmaster/CommandLineUtils/pull/393
[#402]: https://github.com/natemcmaster/CommandLineUtils/pull/402
[#409]: https://github.com/natemcmaster/CommandLineUtils/pull/409
[#415]: https://github.com/natemcmaster/CommandLineUtils/pull/415

## [v3.0.0](https://github.com/natemcmaster/CommandLineUtils/compare/v2.6.0...v3.0.0)

### Improvements

* [@AlexeyEvlampiev] - implement support for GUID command line options ([#333]) and any other type that TypeConverter supports ([#62])
* [@vpkopylov] - Added support for using the 'builder API' with generic host ([#351])
* [@natemcmaster] - Disabled the pager for help text by default. To re-enable, set UsePagerForHelpText = true. ([#346])
* [@natemcmaster] - Fix PlatformNotSupportedException on Xamarin of macOS ([#344])

### Breaking changes

See <https://natemcmaster.github.io/CommandLineUtils/v3.0/upgrade-guide.html> for more details.

* Fix [#251] by [@mattnischan] - remove API that was marked as obsolete in 2.x releases
* Fix [#294] by [@natemcmaster] - change dependencies on McMaster.Extensions.Hosting.CommandLine to just use Microsoft.Extensions.Hosting.Abstractions
* Fix [#337] by [@natemcmaster] - removed .NET Standard 1.6 target from library

[@AlexeyEvlampiev]: https://github.com/AlexeyEvlampiev
[@mattnischan]: https://github.com/mattnischan
[@natemcmaster]: https://github.com/natemcmaster

[#62]: https://github.com/natemcmaster/CommandLineUtils/issues/62
[#251]: https://github.com/natemcmaster/CommandLineUtils/issues/251
[#294]: https://github.com/natemcmaster/CommandLineUtils/issues/294
[#337]: https://github.com/natemcmaster/CommandLineUtils/issues/337
[#333]: https://github.com/natemcmaster/CommandLineUtils/issues/333
[#344]: https://github.com/natemcmaster/CommandLineUtils/issues/344
[#346]: https://github.com/natemcmaster/CommandLineUtils/issues/346
[#351]: https://github.com/natemcmaster/CommandLineUtils/pull/351

## [v2.6.0](https://github.com/natemcmaster/CommandLineUtils/compare/v2.5.1...v2.6.0)

* Refactor: obsolete throwOnUnexpectedArg in favor of UnrecognizedArgumentHandling. See https://github.com/natemcmaster/CommandLineUtils/issues/339 for details

## [v2.5.1](https://github.com/natemcmaster/CommandLineUtils/compare/v2.5.0...v2.5.1)

* Fix [#320] - lazy load command object types to reduce unnecessary allocations

[#320]: https://github.com/natemcmaster/CommandLineUtils/issues/320

## [v2.5.0](https://github.com/natemcmaster/CommandLineUtils/compare/v2.4.4...v2.5.0)

* Fix [#92] by [@kbilsted] - Show enum names in help text for Options and Arguments stored as enum
* Fix [#287] by [@Alxandr] - Add support for private base type options
* Fix [#303] by [@AndreGleichner] - Update generated help to display [command] first then [options]
* Fix [#307] by [@daveMueller] - Update help text generator to display HelpOption correctly

[@kbilsted]: https://github.com/kbilsted
[@Alxandr]: https://github.com/Alxandr
[@AndreGleichner]: https://github.com/AndreGleichner
[@daveMueller]: https://github.com/daveMueller
[#92]: https://github.com/natemcmaster/CommandLineUtils/issues/92
[#287]: https://github.com/natemcmaster/CommandLineUtils/issues/287
[#303]: https://github.com/natemcmaster/CommandLineUtils/issues/303
[#307]: https://github.com/natemcmaster/CommandLineUtils/issues/307

## [v2.4.4](https://github.com/natemcmaster/CommandLineUtils/compare/v2.4.3...v2.4.4)

* Fix [#302] - fix regression introduced in 2.4.2 which caused Ctrl+C handling not to terminate the process quickly

[#302]: https://github.com/natemcmaster/CommandLineUtils/issues/302

## [v2.4.3](https://github.com/natemcmaster/CommandLineUtils/compare/v2.4.2...v2.4.3)

* Fix [#292] by [@thomaslevesque] - fix deadlock when `Environment.Exit` is called

[#292]: https://github.com/natemcmaster/CommandLineUtils/issues/292
[@thomaslevesque]: https://github.com/thomaslevesque

## [v2.4.2](https://github.com/natemcmaster/CommandLineUtils/compare/v2.4.1...v2.4.2)

* Fix [#286] - fix deadlock in CTRL+C handling on Windows

[#286]: https://github.com/natemcmaster/CommandLineUtils/issues/286

## [v2.4.1](https://github.com/natemcmaster/CommandLineUtils/compare/v2.4.0...v2.4.1)

* Fix [#277] - workaround a bug in NuGet's 'deterministic packaging' feature which causes issues based on your timezone

[#277]: https://github.com/natemcmaster/CommandLineUtils/issues/277

## [v2.4.0](https://github.com/natemcmaster/CommandLineUtils/compare/v2.3.4...v2.4.0)

* Fix [#227] by [@ejball] - ArgumentEscaper should escape empty string
* PR [#230] by [@IanG] - Attributes for files and directories that must not exist
* Fix [#221] by [@vpkopylov] - Use Pager for help text option only works on top-level help
* PR [#239] by [@vpkopylov] - Add check for subcommand cycle
* Support C# 8.0 and nullable reference types - [#245]
* Add async methods to CommandLineApplication
* Fix [#208] - make `CommandLineApplication.ExecuteAsync` actually asynchronous
* Fix [#153] - add async methods that accept cancellation tokens
* Fix [#111] - Handle CTRL+C by default
* Fix [#246] by [@kyle-rader] - Multi-line option descriptions do not indent correctly
* Fix [#253] - support calling CommandLineApplication.Execute multiple times

[#246]: https://github.com/natemcmaster/CommandLineUtils/issues/246
[#111]: https://github.com/natemcmaster/CommandLineUtils/issues/111
[#153]: https://github.com/natemcmaster/CommandLineUtils/issues/153
[#208]: https://github.com/natemcmaster/CommandLineUtils/issues/208
[#221]: https://github.com/natemcmaster/CommandLineUtils/issues/221
[#227]: https://github.com/natemcmaster/CommandLineUtils/issues/227
[#253]: https://github.com/natemcmaster/CommandLineUtils/issues/253
[#230]: https://github.com/natemcmaster/CommandLineUtils/pull/230
[#239]: https://github.com/natemcmaster/CommandLineUtils/pull/239
[#245]: https://github.com/natemcmaster/CommandLineUtils/pull/245

## [v2.3.4]

Bugs fixed:

* Fix [#218]: Handle options with multiple characters in the short option name when only specified in a subcommand
* PR [#224] by [@SteveBenz]: Rearrange the order that commands are listed in the USAGE block and list all the commands on the line

[#218]: https://github.com/natemcmaster/CommandLineUtils/issues/218
[#224]: https://github.com/natemcmaster/CommandLineUtils/pull/224

## [v2.3.3]

Enhancements:
* [@mpipo]: add an API to disable the pager for help text (CommandLineApplication.UsePagerForHelpText) ([#216])

[#216]: https://github.com/natemcmaster/CommandLineUtils/pull/216

## [v2.3.2]

Enhancements:
* Fix [#211] by [@rlvandaveer]: honor attributes on classes which implement ValidationAttribute

Bugs fixed:
* Fix [#207] by [@jcaillon]: Option for the case sensitivity of command names

[#207]: https://github.com/natemcmaster/CommandLineUtils/issues/207
[#211]: https://github.com/natemcmaster/CommandLineUtils/issues/211

## [v2.3.1]

Bugs fixed:
* Fix [#203] - fix InvalidOperationException thrown during help text generation on Mono

[#203]: https://github.com/natemcmaster/CommandLineUtils/issues/203

## [v2.3.0]

**Dec. 31, 2018**

Enhancements:
* PR [#192] by [@TheConstructor]: Add IUnhandledExceptionHandler

Bugs fixed:
* Fix [#195]: don't use Task.Run in generic host

[#192]: https://github.com/natemcmaster/CommandLineUtils/pull/192
[#195]: https://github.com/natemcmaster/CommandLineUtils/issues/195

## [v2.3.0-rc]

Bugs fixed:
* Fix [#189] by [@TheConstructor]: fix inference about clustering options for sub-sub-commands

Enhancements:
* Fix [#166] by [@TheConstructor]: make CommandLineApplication.GetValidationResult() public
* PR [#192] by [@TheConstructor]: improve error handling in generic host, and unwrap TargetInvocationException

[#189]: https://github.com/natemcmaster/CommandLineUtils/pull/189
[#166]: https://github.com/natemcmaster/CommandLineUtils/pull/166
[#192]: https://github.com/natemcmaster/CommandLineUtils/pull/192

## [v2.3.0-beta]

Bugs fixed:

* Fix [#86] by [@handcraftedsource]: handled nested HelpOptions. Fixes InvalidOperationException when multiple help options were defined.([#158])
* Fix [#163] - Obsolete CommandOption.Template and fix help text generation to accurately list available options
* [@TheConstructor]: fixed a bug in ConstructorInjectionConvention ([#181])

[#86]: https://github.com/natemcmaster/CommandLineUtils/pull/86
[#158]: https://github.com/natemcmaster/CommandLineUtils/pull/158
[#163]: https://github.com/natemcmaster/CommandLineUtils/pull/163
[#181]: https://github.com/natemcmaster/CommandLineUtils/pull/181

Enhancements:

* [@jcaillon]: Add a new API `UnrecognizedCommandParsingException` which includes suggestions for similar options or
  commands. ([#164])
* Add support counting of bool/flag variables using `bool[]`. ([#143])
* [@EricStG]: Add a new API `MissingParameterlessConstructorException` that includes the type causing the exception in the message. ([#148])
* [@atifaziz]: Add a new API `ValueParser.Create` which makes it easier to create implementations of `IValueParser`
    ([#169])
* [@TheConstructor]: Support injection of IConsole and IConvention in generic host ([#178])


[#143]: https://github.com/natemcmaster/CommandLineUtils/pull/143
[#164]: https://github.com/natemcmaster/CommandLineUtils/pull/164
[#168]: https://github.com/natemcmaster/CommandLineUtils/pull/168
[#169]: https://github.com/natemcmaster/CommandLineUtils/pull/169
[#178]: https://github.com/natemcmaster/CommandLineUtils/pull/178

Other notes:
* I adjusted some API released in the alpha - primarily, I removed ParserSettings.

### New package: McMaster.Extensions.Hosting.CommandLine

Thanks to [@lucastheisen] for writing a new package, McMaster.Extensions.Hosting.CommandLine ([#167]). This new package provides
integration with ASP.NET Core's ["Generic Host" feature.](https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host).

[#167]: https://github.com/natemcmaster/CommandLineUtils/pull/167

## [v2.3.0-alpha]

Enhancements:

* Support the POSIX convention for cluster multiple options. For example, `-ixd` is treated the same as `-i -x -d`.
  Resolved [#93].
* [@bjorg]: support SingleOrNoValue notation. `--option[:value]`
* New type: `Pager`. Provides a simple interaction model for displaying console output in a pager.
* Display help text using the `less` pager on macOS/Linux.
* Make suggestions when invalid options or commands are used, but there is a valid one that is similar.
  (Thanks to [@MadbHatter] for doing the initial work on this.)
* Add support for subcommand aliases. Commands can be given multiple names.

  ```c#
  [Command("organization", "org", "o")]
  public class OrgCommand { }
  ```
* Obsolete the constructor of `[Subcommand]` which takes a string.
* Infer subcommand names from the type name
  ```c#
  [Subcommand(typeof(AddCommand))]
  public class Git { }

  public class AddCommand { } // subcommand name = "add"
  ```
* [@lvermeulen]: Sort subcommands by name in help text. This can be disabled with `DefaultHelpTextGenerator.SortCommandsByName`.

[#93]: https://github.com/natemcmaster/CommandLineUtils/issues/93

Bugs fixed:

* Duplicate subcommand names used to cause undefined behavior. Now, attempting to add a duplicate subcommand name or aliases will
  cause the library to throw before the app can execute.
* Fix bug in subcommand name inference. When not specified, the subcommand always matched the entry assembly name.
  In this update, this convention only applies to the parent command.
* Fix [#131](https://github.com/natemcmaster/CommandLineUtils/issues/131) - Add generic overloads of `.IsRequired()` for
 `CommandOption<T>` and `CommandArgument<T>`.

Details:

* **Clustering options:** I've added this behavior as the new default, but only if I think it won't interfere with existing apps.
  If it causes issues or you don't like clustering, you can disable this by setting
  `CommandLineApplication.ClusterOptions = false`.
  To preserve compatibility with existing apps, this behavior is off if you have configured options with short names with
  multiple characters. In a future version, this will cause an error unless you set `ClusterOptions = false`.

* **Pager:** this is the new default for showing help text. The pager should have graceful fallback to regular stdout
  when there are issues launching `less`, or when stdout is redirected into a pipe.

## [v2.2.5]

**July 1, 2018**

Bug fixes:
 * [@bording] and [@SeanFeldman]: Unable to create new instance of `CommandOption<T>` for type that is already registered

Minor improvements:
 * Started code signing the NuGet package and binaries
 * [@jerriep]: added documentation for response file parsing

## [v2.2.4]

**May 24, 2018**

Bug fixes:

* [@liamdawson]: support parsing `System.Uri`
* Fix #101 - Update `DotNetExe.FullPath` to honor the `DOTNET_ROOT` environment variable as a fallback

## [v2.2.3]

**May 10, 2018**

Minor improvements:

* Make it easier to override the version text shown when `--version` is specified.
* Make DefaultHelpTextGenerator's constructor protected
* Fix DebugHelper.HandleDebugSwitch

## [v2.2.2]

**April 28, 2018**

Minor improvement:

 - Add conventions from attributes that implement IConvention and IMemberConvention
 - Add a help option by default as long as it doesn't conflict with existing options

## [v2.2.1]

**April 10, 2018**

Bug fixes:

 - Don't assign option and argument options if no value was provided, preserving the default CLR value unless there is user-input.
 - Fix ShowHint() to use ShortName or SymbolName if OptionHelp.LongName is not set
 - Fix [#85](https://github.com/natemcmaster/CommandLineUtils/issues/85) - lower priority of resolving AdditionalServices after most built-in services
 - Fix [#79](https://github.com/natemcmaster/CommandLineUtils/issues/79) - OnValidate callbacks invoked before property values were assigned

Minor improvements:

 - Improve help text generation. Align columns, show top-level command description, and add `protected virtual` API to `DefaultHelpTextGenerator` to make it easier to customize help text

## [v2.2.0]

**March 30, 2018**

 - Added support for command validators using `CommandLineApplication.Validators` and added a new OnValidate convention
 - Fix minor bug in `ArgumentEscaper` where some strings were not properly escaped
 - Update to System.ComponentModel.Annotations 4.4.1 (netstandard2.0 only)
 - [@atruskie]: Ensure ValueParsers are inherited in subcommands

## [v2.2.0-rc]

**March 23, 2018**

New API:
 - [@atruskie]: Added support for user-defined value parsers using `IValueParser` and `CommandLineApplication.ValueParsers`.
 - Added support for `Option<T>` and `Argument<T>`
 - Added `IValidationBuilder<T>`
 - Added `.Accepts().Range(min, max)` for int and double type arguments and options

Enhancements:
 - Parse these values to boolean: T, t, F, f, 0, 1

Removed:
 - Removed support for mapping `Tuple<bool,T>` to `CommandOptionType.SingleOrNoValue`.
   `ValueTuple<bool,T>` is still present.

## [v2.2.0-beta]

**March 7, 2018**

New features:
  - Add `CommandOptionType.SingleOrNoValue`. Options of this type can be a switch, or have a value but only in the form `--param:value` or `--param=value`.
  - Support mapping `Tuple<bool,T>` and `ValueTuple<bool,T>` to `CommandOptionType.SingleOrNoValue`
  - Added `CommandLineApplication<TModel>`. This allows associating an application with a specific .NET type
  - Convention API. Adds support for writing your own conventions to convert command line arguments into a .NET type
  - [@sebastienros] - Support for case-insensitive options
  - Add support for constructor injection and dependency injection by providing a custom service provider

## [v2.2.0-alpha]

**Feb. 15, 2018**

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


[@atifaziz]: https://github.com/atifaziz
[@atruskie]: https://github.com/atruskie
[@bjorg]: https://github.com/bjorg
[@bording]: https://github.com/bording
[@cbcrouse]: https://github.com/cbcrouse
[@chaami]: https://github.com/chaami
[@couven92]: https://github.com/couven92
[@cuperman007]: https://github.com/cuperman007
[@danielmeza]: https://github.com/danielmeza
[@demosdemon]: https://github.com/demosdemon
[@ejball]: https://github.com/ejball
[@EricStG]: https://github.com/EricStG
[@handcraftedsource]: https://github.com/handcraftedsource
[@hellfirehd]: https://github.com/hellfirehd
[@IanG]: https://github.com/IanG
[@jakubqwe]: https://github.com/jakubqwe
[@jcaillon]: https://github.com/jcaillon
[@jerriep]: https://github.com/jerriep
[@kant2002]: https://github.com/kant2002
[@kyle-rader]: https://github.com/kyle-rader
[@liamdawson]: https://github.com/liamdawson
[@lucastheisen]: https://github.com/lucastheisen
[@lvermeulen]: https://github.com/lvermeulen
[@MadbHatter]: https://github.com/MadbHatter
[@mpipo]: https://github.com/mpipo
[@rlvandaveer]: https://github.com/rlvandaveer
[@rmcc13]: https://github.com/rmcc13
[@scott-xu]: https://github.com/scott-xu
[@SeanFeldman]: https://github.com/SeanFeldman
[@sebastienros]: https://github.com/sebastienros
[@skirchner989]: https://github.com/skirchner989
[@SteveBenz]: https://github.com/SteveBenz
[@TheConstructor]: https://github.com/TheConstructor
[@vpkopylov]: https://github.com/vpkopylov
[@xoofx]: https://github.com/xoofx

[v2.3.4]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.3.3...v2.3.4
[v2.3.3]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.3.2...v2.3.3
[v2.3.2]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.3.1...v2.3.2
[v2.3.1]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.3.0...v2.3.1
[v2.3.0]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.5...v2.3.0
[v2.3.0-rc]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.5...v2.3.0-rc
[v2.3.0-beta]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.5...v2.3.0-beta
[v2.3.0-alpha]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.5...v2.3.0-alpha
[v2.2.5]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.4...v2.2.5
[v2.2.4]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.3...v2.2.4
[v2.2.3]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.3...v2.2.3
[v2.2.2]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.1...v2.2.2
[v2.2.1]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.0...v2.2.1
[v2.2.0]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.0-rc...v2.2.0
[v2.2.0-rc]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.0-beta...v2.2.0-rc
[v2.2.0-beta]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.2.0-alpha...v2.2.0-beta
[v2.2.0-alpha]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.1.1...v2.2.0-alpha
[v2.1.1]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.1.0...v2.1.1
[v2.1.0]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.0.1...v2.1.0
[v2.0.1]: https://github.com/natemcmaster/CommandLineUtils/compare/v2.0.0...v2.0.1
[v2.0.0]: https://github.com/natemcmaster/CommandLineUtils/compare/b0c662d331c35ccf3145875cdef850df7e896c0f...v2.0.0
