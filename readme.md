# Csproj

Csproj is a tool that allows bulk modification of .csproj files. It provides a simple command-line interface to perform various operations on multiple .csproj files at once. It supports both sln and slnx formats.

It can be installed with dotnet tool:

```
dotnet tool install -g csproj
```

## Enable or disable implicit usings

```
USAGE:
    csproj modify implicitusings <path> [OPTIONS]

EXAMPLES:
    csproj csproj implicitusings solution.sln --enable

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help      Prints help information
    -b, --backup    Create a backup of the modified project file(s)
    -f, --force     Force run, even if not in a git repo
    -e, --enable    Enable or disable nullable reference types
```

## Set the C# language version for the project

```
USAGE:
    csproj modify langversion <path> [OPTIONS]

EXAMPLES:
    csproj csproj langversion solution.slnx -v preview

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help       Prints help information
    -b, --backup     Create a backup of the modified project file(s)
    -f, --force      Force run, even if not in a git repo
    -v, --version    Set the C# language version. Can be a major.minor number or preview, latest, latestmajor
``` 

## Enable or disable nullable reference types

```
USAGE:
    csproj modify nullable <path> [OPTIONS]

EXAMPLES:
    csproj csproj nullable c:\folder --enable

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help      Prints help information
    -b, --backup    Create a backup of the modified project file(s)
    -f, --force     Force run, even if not in a git repo
    -e, --enable    Enable or disable nullable reference types
```

## Set the target framework for the project

```
USAGE:
    csproj modify targetframework <path> [OPTIONS]

EXAMPLES:
    csproj csproj targetframework solution.slnx --old net8.0 --target net10.0

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help      Prints help information
    -b, --backup    Create a backup of the modified project file(s)
    -f, --force     Force run, even if not in a git repo
    -t, --target    The target framework to upgrade to
    -o, --old       The old target framework to upgrade from
```

## Set project versions

```
USAGE:
    csproj modify version <path> [OPTIONS]

EXAMPLES:
    csproj modify version solution.slnx -v 1.0.0.0

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help        Prints help information
    -b, --backup      Create a backup of the modified project file(s)
    -f, --force       Force run, even if not in a git repo
    -v, --version     The version prefix to set
        --file        The file version to set
        --assembly    The assembly version to set
```

## Convert projects to use central package management

```
USAGE:
    csproj modify enable-cpm <path> [OPTIONS]

EXAMPLES:
    csproj modify enable-cpm solution.slnx

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help      Prints help information
    -b, --backup    Create a backup of the modified project file(s)
    -f, --force     Force run, even if not in a git repo
```

## Convert projects to not use central package management

```
USAGE:
    csproj modify disable-cpm <path> [OPTIONS]

EXAMPLES:
    csproj modify disable-cpm solution.slnx

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help      Prints help information
    -b, --backup    Create a backup of the modified project file(s)
    -f, --force     Force run, even if not in a git repo
```

## Inspect and remove redundant project/NuGet references in solution projects

```
USAGE:
    csproj modify prune-links <path> [OPTIONS]

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help        Prints help information
    -b, --backup      Create a backup of the modified project file(s)
    -f, --force       Force run, even if not in a git repo
    -D, --dryrun      Dryrun mode. Only show what would be changed
    -v, --verbose     Show the reference tree for each project
        --graph-md    Output the dependency graph as a Markdown file with Mermaid syntax
```

## List all NuGet package references in the project(s)

```
USAGE:
    csproj info nugets <path> [OPTIONS]

EXAMPLES:
    csproj info nugets solution.sln

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help    Prints help information
```

## Visualize project or package dependencies

```
USAGE:
    csproj info dependencies <path> [OPTIONS]

EXAMPLES:
    csproj info dependencies solution.sln -d project

ARGUMENTS:
    <path>    Solution or project file name or a directory containing one

OPTIONS:
    -h, --help               Prints help information
    -d, --dependency-type    Dependency type to visualize. Can be project or package
    -o, --output             Output type, can be console, mermaid or nomnoml
```