using CsProj.Commands.Info;
using CsProj.Commands.Modify;
using CsProj.Domain;
using CsProj.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;
using Spectre.Console.Cli;

var registrations = new ServiceCollection()
    .AddSingleton(AnsiConsole.Console)
    .AddSingleton(TimeProvider.System)
    .AddSingleton<ILogger, ConsoleLogger>();

var typeRegistar = new TypeRegistrar(registrations);

Console.OutputEncoding = System.Text.Encoding.UTF8;

CommandApp app = new(typeRegistar);

app.Configure(config =>
{
    config.SetApplicationName("csproj");

    config.AddBranch("modify", modify =>
    {
        modify.SetDescription("Modify various properties of multiple projects");

        modify
            .AddCommand<NullableCommand>("implicitusings")
            .WithDescription("Enable or disable implicit usings");

        modify
            .AddCommand<LangVersionCommand>("langversion")
            .WithDescription("Set the C# language version for the project");

        modify
            .AddCommand<NullableCommand>("nullable")
            .WithDescription("Enable or disable nullable reference types");

        modify
            .AddCommand<TargetFrameworkCommand>("targetframework")
            .WithDescription("Set the target framework for the project");

        modify
            .AddCommand<VersionCommand>("version")
            .WithDescription("Set project versions");

        modify
            .AddCommand<UseCentralPackageManagementCommand>("use-cpm")
            .WithDescription("Convert projects to use central package management");
    });

    config.AddBranch("info", info =>
    {
        info.SetDescription("Display various information about multiple projects");

        info
            .AddCommand<NugetsCommand>("nugets")
            .WithDescription("List all NuGet package references in the project(s)");

        info
            .AddCommand<DependenciesCommand>("dependencies")
            .WithDescription("Visualize project or package dependencies");
    });
});

return await app.RunAsync(args);