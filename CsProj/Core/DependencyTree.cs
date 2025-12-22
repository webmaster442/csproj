namespace CsProj.Core;

internal class Dependency
{
    public required string Name { get; set; }
    public string AdditionalInfo { get; set; } = string.Empty;
    public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
}

internal class Dependecies : List<Dependency>
{
    public static string ConvertToNomNoml()
    {

    }

    public static string ConvertToMermaid()
    {

    }
}