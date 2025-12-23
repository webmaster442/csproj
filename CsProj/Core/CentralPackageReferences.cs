using System.Xml.Linq;

using NuGet.Versioning;

namespace CsProj.Core;

internal static class CentralPackageReferences
{
    public static string CreateXml(IReadOnlyDictionary<string, NuGetVersion> packages)
    {
        var doc = new XDocument(
             new XElement("Project",
                 new XElement("PropertyGroup",
                     new XComment(" Enable central package management, https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management "),
                     new XElement("ManagePackageVersionsCentrally", "true")
                 ),
                 new XElement("ItemGroup", CreatePackages(packages))
             )
         );

        return doc.ToString();
    }

    private static XElement[] CreatePackages(IReadOnlyDictionary<string, NuGetVersion> packages)
    {
        XElement[] result = new XElement[packages.Count];

        int index = 0;
        foreach (var package in packages.OrderBy(x => x.Key))
        {
            result[index] = new XElement("PackageVersion",
                                         new XAttribute("Include", package.Key),
                                         new XAttribute("Version", package.Value.ToString()));
            ++index;
        }

        return result;
    }
}