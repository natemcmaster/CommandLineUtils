---
description: Prepare release notes for CommandLineUtils NuGet packages
---

# Prepare Release

Analyzes git history, categorizes changes, and generates dual-format release notes for `releasenotes.props` (NuGet) and `CHANGELOG.md` (GitHub).

## Workflow

When preparing a release (invoked via `/prepare-release [version]` or automatically when context indicates release preparation):

### 1. Determine Version and Scope

- Parse the version number (ask if not provided)
- Identify type: **Major** (X.0.0), **Minor** (X.Y.0), or **Patch** (X.Y.Z)
- Find previous version tag for comparison

### 2. Gather Changes

Run git commands to analyze commits since last release:
- `git log` for commit history
- `git log --grep` for PR merges
- Look for conventional commit patterns: `fix:`, `feat:`, `break:`, `docs:`, etc.

### 3. Categorize Changes

Group into categories (in this order):

1. **Breaking changes** (major versions only) - API removals, behavior changes
2. **Features** - New functionality or APIs
3. **Fixes** - Bug fixes and corrections
4. **Improvements** - Performance or usability enhancements
5. **Docs** - Documentation-only changes
6. **Other** - Infrastructure, tooling, CI/CD

### 4. Generate releasenotes.props Entry

**Format:** `* @contributor: description (#PR)`

**XML escaping:** `<` → `&lt;`, `>` → `&gt;`, `&` → `&amp;`

**Minor/Major versions:**
```xml
<PackageReleaseNotes Condition="$(VersionPrefix.StartsWith('X.Y.'))">
Changes since X.Y-1:

Breaking changes:
* @user: description (#123)

Features:
* @user: description (#124)

Fixes:
* @user: description (#125)

See more details here: https://github.com/natemcmaster/CommandLineUtils/blob/main/CHANGELOG.md#vXYZ
</PackageReleaseNotes>
```

**Patch versions:**
```xml
<PackageReleaseNotes Condition="'$(VersionPrefix)' == 'X.Y.Z'">
$(PackageReleaseNotes)

X.Y.Z patch:
* @user: fix description (#123)
</PackageReleaseNotes>
```

### 5. Generate CHANGELOG.md Entry

**Format:** `* [@contributor]: description ([#PR])`

```markdown
## [vX.Y.Z](https://github.com/natemcmaster/CommandLineUtils/compare/vX.Y.Z-1...vX.Y.Z)

### Features
* [@user]: description ([#123])

### Fixes
* [@user]: description ([#124])

[#123]: https://github.com/natemcmaster/CommandLineUtils/pull/123
[#124]: https://github.com/natemcmaster/CommandLineUtils/pull/124
```

Insert after "Unreleased changes" line.

### 6. Update Directory.Build.props

Update `<VersionPrefix>` to match release version.

### 7. Present for Review

Show the user:
- Summary of changes by category
- Proposed `releasenotes.props` addition
- Proposed `CHANGELOG.md` addition
- `Directory.Build.props` version update

Ask for confirmation before applying changes.

### 8. Apply Changes

Once approved, edit the three files.

### 9. Final Checklist

Remind user to:
- Review generated notes for accuracy
- Run `./build.ps1` to verify build
- Create git tag: `git tag vX.Y.Z`
- Push tag: `git push origin vX.Y.Z`

## Style Guide

### Contributor Attribution

Always credit contributors:
```
* @username: description (#123)
```

For multiple contributors:
```
* @user1 and @user2: description (#123 and #456)
```

### Description Format

**Good examples:**
- `fix: find dotnet.exe correctly when DOTNET_ROOT is not set`
- `feature: add API for setting default value on options and arguments`
- `don't mask OperationCanceledException triggered by SIGINT`

**Avoid:**
- Implementation details: ~~"Refactored ValidationAttribute processing"~~
- Vague: ~~"Fixed bug"~~, ~~"Updated code"~~
- Missing attribution: ~~"Fixed parser (#123)"~~

### Optional Prefixes

Use when it adds clarity (optional under category headings):
- `fix:` - Bug fixes
- `feature:` - New features
- `cleanup:` - Code cleanup
- `docs:` - Documentation
- `bugfix:` - Alternative to "fix:"

### PR References

```
(#123)                    # single PR
(#389 and #420)          # multiple PRs
```

Omit if direct commit (no PR).

## Version-Specific Patterns

### Major Versions (X.0.0)

Lead with breaking changes prominently. Consider linking to upgrade guide.

```
Changes since X-1.Y.Z:

Breaking changes:
* @user: remove deprecated API (#251)
* @user: dropped .NET Standard 1.6 support (#337)

Features:
* [new features]

See https://natemcmaster.github.io/CommandLineUtils/vX.0/upgrade-guide.html
```

### Patch Versions

Use `$(PackageReleaseNotes)` to inherit parent version's notes.

For first patch (X.Y.1), create new conditional entry after parent.
For subsequent patches, add BEFORE existing patches but AFTER minor version.

## Quality Checklist

- [ ] All changes have contributor attribution (@username)
- [ ] All changes have PR references when applicable (#123)
- [ ] XML special characters escaped (&lt; &gt; &amp;)
- [ ] Categories in correct order
- [ ] Version condition is exact
- [ ] Footer link included and correct (version without dots)
- [ ] CHANGELOG.md has markdown link references
- [ ] Descriptions are clear and user-focused

## Examples

See `src/CommandLineUtils/releasenotes.props`:
- Version 4.0.0 - Major with breaking changes
- Version 4.0.1 - First patch
- Version 4.0.2 - Second patch
- Version 3.1.0 - Minor with features
