using Csproj.Commands;

using Spectre.Console.Cli;

CommandApp app = new();

app.Configure(cfg =>
{
    cfg.SetApplicationName("csproj");

    cfg.AddCommand<TargetFrameworkCommand>("targetframework")
       .WithDescription("Set the target framework for the project");

    cfg.AddCommand<NullabilityCommand>("nullable")
       .WithDescription("Set the nullable context for the project");
});

app.Run(args);