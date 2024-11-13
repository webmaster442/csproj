# csproj

csproj is a simple tool to change properties of multiple C# projects

## Installation

```bash
dotnet tool install -g csproj
```

## Provided commands

### targetframework

```
DESCRIPTION:
Set the target framework for the project

USAGE:
    csproj targetframework [OPTIONS]

OPTIONS:
    -h, --help         Prints help information
    -p, --project      Project File Path. Can be a directory or a single project file. If not provided, the current
                       directory is used
    -r, --recursive    Recursive search for csproj files
    -f, --filter       Filter project files by name. Wildcards like * and ? are supported
    -b, --backup       Create a backup of the project file
    -t, --target       The target framework to upgrade to
    -o, --old          The old target framework to upgrade from
```

Example: upgrade all csproj files in the current directory and subdirectories from netcoreapp3.1 to net9.0 recursively and create a backup of the original files:

```bash
csproj targetframework --recursive --backup --old netcoreapp3.1 --target net9.0
```

Example: upgrade all csproj files in the current directory from netcoreapp3.1 to net9.0 which have the word "Web" in their name:

```bash
csproj targetframework --filter "Web*" --old netcoreapp3.1 --target net9.0
```

### nullable

```
DESCRIPTION:
Set the nullable context for the project

USAGE:
    csproj nullable [OPTIONS]

OPTIONS:
    -h, --help           Prints help information
    -p, --project        Project File Path. Can be a directory or a single project file. If not provided, the current
                         directory is used
    -r, --recursive      Recursive search for csproj files
    -f, --filter         Filter project files by name. Wildcards like * and ? are supported
    -b, --backup         Create a backup of the project file
    -n, --nullability    The nullability to upgrade to
```

