using Spectre.Console.Cli;

Console.OutputEncoding = System.Text.Encoding.UTF8;

CommandApp app = new();

app.Configure(cfg =>
{
    cfg.SetApplicationName("csproj");

    cfg.AddExample(["targetframework", "-t", "net9.0", "-o", "net8.0", "d:\\project\\test.sln"]);
    cfg.AddExample(["nullable", "-e", "d:\\project.sln"]);
    cfg.AddExample(["version", "--assembly", "1.2.3.4"]);

    cfg.AddCommand<Csproj.Commands.TargetFramework>("targetframework")
       .WithDescription("Set the target framework for the project");

    cfg.AddCommand<Csproj.Commands.Nullable>("nullable")
       .WithDescription("Enable or disable nullable reference types");

    cfg.AddCommand<Csproj.Commands.Version>("version")
        .WithDescription("Set project versions");

    cfg.AddBranch("licenseheaders", headers =>
    {
        headers.AddCommand<Csproj.Commands.HeadersApply>("apply")
               .WithDescription("Apply license headers based on specified template");

        headers.AddCommand<Csproj.Commands.HeadersCreate>("create")
               .WithDescription("Create a default license headers template file (licenses.xml)");
    });
});

app.Run(args);