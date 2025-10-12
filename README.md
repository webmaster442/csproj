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

### version

```
DESCRIPTION:
Set project, file, and assembly versions

USAGE:
    csproj version [OPTIONS]

OPTIONS:
    -h, --help             Prints help information
    -p, --project          Project File Path. Can be a directory or a single project file. If not provided, the current directory is used
    -r, --recursive        Recursive search for csproj files
    -f, --filter           Filter project files by name. Wildcards like * and ? are supported
    -b, --backup           Create a backup of the project file
    -v, --version          The version prefix to set (e.g. 1.2.3.4)
    -a, --assembly         The assembly version to set (e.g. 1.2.3.4)
    -f, --file             The file version to set (e.g. 1.2.3.4)
```

Example: set all project versions in the current directory to 1.2.3.4:

```bash
csproj version --version 1.2.3.4
```

Example: set assembly and file versions for all projects recursively:

```bash
csproj version --recursive --assembly 1.2.3.4 --file 1.2.3.4
```

### licenseheaders

#### apply

```
DESCRIPTION:
Apply license headers to source files using a specified template

USAGE:
    csproj licenseheaders apply [OPTIONS]

OPTIONS:
    -h, --help             Prints help information
    -d, --directory        Directory to process. Default: current working directory
    -t, --template         License template file (default: licenses.xml in current directory)
    -D, --dryrun           Dryrun mode. Don't do any changes, just output what would happen
    -r, --recursive        Process subdirectories as well
```

Example: apply license headers recursively using a custom template:

```bash
csproj licenseheaders apply --directory src --template my_license.xml --recursive
```

#### create

```
DESCRIPTION:
Create a default license headers template file (licenses.xml)

USAGE:
    csproj licenseheaders create
```

Example: create a default license header template in the current directory:

```bash
csproj licenseheaders create
```

### prunelinks

```
DESCRIPTION:
Inspect and remove redundant project/NuGet references in solution projects. Optionally outputs a dependency graph in Markdown/Mermaid format.

USAGE:
    csproj prunelinks [OPTIONS] --solution <SolutionFile.sln>

OPTIONS:
    -h, --help           Prints help information
    -s, --solution       Solution file path (.sln)
    -D, --dryrun         Only show what would be changed, do not modify files
    -b, --backup         Create a backup of the project file before editing
    -v, --verbose        Show the reference tree for each project
    --graph-md [file]    Output the dependency graph as a Markdown file with Mermaid syntax. If no file is specified, defaults to <SolutionName>.md in the solution directory.
```

#### Example: Remove redundant links and output dependency graph

```bash
csproj prunelinks --solution MySolution.sln --graph-md
```
This will create `MySolution.md` in the same directory as the solution, containing a Mermaid graph of project dependencies.

#### Example: Specify a custom Markdown output file

```bash
csproj prunelinks --solution MySolution.sln --graph-md dependencies.md
```

#### Example Mermaid Markdown output

```markdown
# MySolution.md

````mermaid
---
title: MySolution
---
graph TD
    ProjectA --> ProjectB
    ProjectB --> ProjectC
    ProjectA --> ProjectC
````

That gives you a visual representation of the project dependencies in your solution. You can open this Markdown file in any compatible viewer that supports Mermaid diagrams to see the graph rendered visually.

````mermaid
---
title: MySolution
---
graph TD
    ProjectA --> ProjectB
    ProjectB --> ProjectC
    ProjectA --> ProjectC
````

