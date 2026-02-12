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
            .WithDescription("Enable or disable implicit usings")
            .WithExample("modify implicitusings solution.sln --enable");

        modify
            .AddCommand<LangVersionCommand>("langversion")
            .WithDescription("Set the C# language version for the project")
            .WithExample("modify langversion solution.slnx -v preview");

        modify
            .AddCommand<NullableCommand>("nullable")
            .WithDescription("Enable or disable nullable reference types")
            .WithExample("modify nullable c:\\folder --enable");

        modify
            .AddCommand<TargetFrameworkCommand>("targetframework")
            .WithDescription("Set the target framework for the project")
            .WithExample("modify targetframework solution.slnx --old net8.0 --target net10.0");

        modify
            .AddCommand<VersionCommand>("version")
            .WithDescription("Set project versions")
            .WithExample("modify version solution.slnx -v 1.0.0.0");

        modify
            .AddCommand<UseCentralPackageManagementCommand>("enable-cpm")
            .WithDescription("Convert projects to use central package management")
            .WithExample("modify enable-cpm solution.slnx");

        modify
            .AddCommand<DontUseCentralPackageManagementCommand>("disable-cpm")
            .WithDescription("Convert projects to not use central package management")
            .WithExample("modify disable-cpm solution.slnx");

        modify
            .AddCommand<PruneLinksCommand>("prune-links")
            .WithDescription("Inspect and remove redundant project/NuGet references in solution projects");
    });

    config.AddBranch("info", info =>
    {
        info.SetDescription("Display various information about multiple projects");

        info
            .AddCommand<NugetsCommand>("nugets")
            .WithDescription("List all NuGet package references in the project(s)")
            .WithExample("info nugets solution.sln");

        info
            .AddCommand<DependenciesCommand>("dependencies")
            .WithDescription("Visualize project or package dependencies")
            .WithExample("info dependencies solution.sln -d project");
    });
});

return await app.RunAsync(args);