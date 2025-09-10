using Spectre.Console.Cli;

Console.OutputEncoding = System.Text.Encoding.UTF8;

CommandApp app = new();

app.Configure(cfg =>
{
    cfg.SetApplicationName("csproj");

    cfg.AddExample(new[] { "targetframework", "-t", "net9.0", "-o", "net8.0", "d:\\project\\test.sln" });

    cfg.AddCommand<Csproj.Commands.TargetFramework>("targetframework")
       .WithDescription("Set the target framework for the project");

    cfg.AddCommand<Csproj.Commands.Nullable>("nullable")
       .WithDescription("Enable or disable nullable reference types");

    cfg.AddCommand<Csproj.Commands.Version>("version")
        .WithDescription("Set project versions");
});

app.Run(args);