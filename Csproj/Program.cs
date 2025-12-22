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

    config
        .AddCommand<CsProj.Commands.TargetFrameworkCommand>("targetframework")
        .WithDescription("Set the target framework for the project");

    config
        .AddCommand<CsProj.Commands.NullableCommand>("nullable")
        .WithDescription("Enable or disable nullable reference types");

    config
        .AddCommand<CsProj.Commands.VersionCommand>("version")
        .WithDescription("Set project versions");

    config
        .AddCommand<CsProj.Commands.ListNugets>("list-nugets")
        .WithDescription("List all NuGet package references in the project(s)");
});

return await app.RunAsync(args);