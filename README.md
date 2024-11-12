# csproj

csproj is a simple tool to change properties of multiple C# projects

## Usage

**Set the target framework of all projects in the current directory to .NET 9.0**

```bash
csproj targetframework -t net9.0
```

**Set the target framework of all projects in the current directory and it's subdirectories to .NET 9.0**

```bash
csproj targetframework --recursive -t net9.0
```

**Enabled nullable reference types for all projects in the current directory**

```bash
csproj nullable -n true
```

## Installation

```bash
dotnet tool install -g csproj
```
