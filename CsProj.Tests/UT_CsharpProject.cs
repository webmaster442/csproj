using System.Xml.Linq;

using CsProj.Core;

namespace CsProj.Tests;

[TestFixture]
public sealed class UT_CsharpProject
{
    private const string SampleCsProjContent =
        """
        <Project Sdk="Microsoft.NET.Sdk">
        

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <LangVersion>latest</LangVersion>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <IsPackable>false</IsPackable>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="coverlet.collector" Version="6.0.4" />
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
            <PackageReference Include="NUnit" Version="4.4.0" />
            <PackageReference Include="NUnit.Analyzers" Version="4.11.2">
              <PrivateAssets>all</PrivateAssets>
              <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            </PackageReference>
            <PackageReference Include="NUnit3TestAdapter" Version="6.0.1" />
          </ItemGroup>

          <ItemGroup>
            <ProjectReference Include="..\CsProj\CsProj.csproj" />
          </ItemGroup>

          <ItemGroup>
            <Using Include="NUnit.Framework" />
          </ItemGroup>

        </Project>
        """;

    private static CsharpProject CrateSut()
        => new("C:\\Projects\\CsProj.Tests\\CsProj.Tests.csproj", SampleCsProjContent);

    [Test]
    public void EnsureThat_Properties_Are_Correctly_Set()
    {
        CsharpProject sut = CrateSut();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.AbsolutePath, Is.EqualTo("C:\\Projects\\CsProj.Tests\\CsProj.Tests.csproj"));
            Assert.That(sut.IsSdkStyleProject, Is.True);
            Assert.That(sut.WasModified, Is.False);
        }
    }

    [Test]
    public void EnsureThat_GetPackageReferences_Works()
    {
        CsharpProject sut = CrateSut();
        var packageReferences = sut.GetPackageReferences().ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(packageReferences, Has.Count.EqualTo(5));
            Assert.That(sut.WasModified, Is.False);
            Assert.That(packageReferences[0].PackageName, Is.EqualTo("coverlet.collector"));
            Assert.That(packageReferences[0].Version, Is.EqualTo(new Version(6, 0, 4)));
        }
    }

    [Test]
    public void EnsureThat_GetProjectReferences_Works()
    {
        CsharpProject sut = CrateSut();
        var projectReferences = sut.GetProjectReferencesAbsolutePath().ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projectReferences, Has.Count.EqualTo(1));
            Assert.That(sut.WasModified, Is.False);
            Assert.That(projectReferences[0], Is.EqualTo("C:\\Projects\\CsProj\\CsProj.csproj"));
        }
    }

    [Test]
    public void EnsureThat_RemovePackageReference_Works()
    {
        CsharpProject sut = CrateSut();
        sut.RemovePackageReference("NUnit.Analyzers");
        var packageReferences = sut.GetPackageReferences().ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(packageReferences, Has.Count.EqualTo(4));
            Assert.That(sut.WasModified, Is.True);
            Assert.That(packageReferences.Any(pr => pr.PackageName == "NUnit.Analyzers"), Is.False);
        }
    }

    [Test]
    public void EnsureThat_RemovePackageReferenceVersion_Works()
    {
        CsharpProject sut = CrateSut();
        sut.RemovePackageReferenceVersion("NUnit");
        var packageReferences = sut.GetPackageReferences().ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(packageReferences, Has.Count.EqualTo(5));
            Assert.That(sut.WasModified, Is.True);
            var nunitPackage = packageReferences.FirstOrDefault(pr => pr.PackageName == "NUnit");
            Assert.That(nunitPackage, Is.Not.Null);
            Assert.That(nunitPackage!.Version, Is.Null);
        }
    }

    [Test]
    public void EnsureThat_SetNullable_Works()
    {
        CsharpProject sut = CrateSut();
        sut.SetNullable(false);

        var nullableElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("Nullable");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(nullableElement, Is.Not.Null);
            Assert.That(nullableElement!.Value, Is.EqualTo("disable"));
        }
    }

    [Test]
    public void EnsureThat_SetImplicitUsings_Works()
    {
        CsharpProject sut = CrateSut();
        sut.SetImplicitUsings(false);

        var implicitUsingsElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("ImplicitUsings");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(implicitUsingsElement, Is.Not.Null);
            Assert.That(implicitUsingsElement!.Value, Is.EqualTo("disable"));
        }
    }

    [Test]
    public void EnsureThat_SetTargetFramework_Works()
    {
        CsharpProject sut = CrateSut();
        sut.SetTargetFramework("net11.0", "net10.0");

        var targetFrameworkElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("TargetFramework");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(targetFrameworkElement, Is.Not.Null);
            Assert.That(targetFrameworkElement!.Value, Is.EqualTo("net11.0"));
        }
    }

    [Test]
    public void EnsureThat_SetTargetFramework_Does_Not_Modify_If_OldFramework_Does_Not_Match()
    {
        CsharpProject sut = CrateSut();
        sut.SetTargetFramework("net11.0", "net12.0");
        var targetFrameworkElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("TargetFramework");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.False);
            Assert.That(targetFrameworkElement, Is.Not.Null);
            Assert.That(targetFrameworkElement!.Value, Is.EqualTo("net10.0"));
        }
    }

    [Test]
    public void EnsureThat_SetVersion_Works()
    {
        CsharpProject sut = CrateSut();
        sut.SetVersion("1.2.3");

        var versionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("Version");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(versionElement, Is.Not.Null);
            Assert.That(versionElement!.Value, Is.EqualTo("1.2.3"));
        }
    }

    [Test]
    public void EnsureThat_SetVersion_Updates_Existing_Version()
    {
        CsharpProject sut = CrateSut();
        sut.SetVersion("1.2.3");
        sut.SetVersion("2.3.4");

        var versionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("Version");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(versionElement, Is.Not.Null);
            Assert.That(versionElement!.Value, Is.EqualTo("2.3.4"));
        }
    }

    [Test]
    public void EnsureThat_SetAssemblyVersion_Works()
    {
        CsharpProject sut = CrateSut();
        sut.SetAssemblyVersion("1.2.3");

        var assemblyVersionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("AssemblyVersion");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(assemblyVersionElement, Is.Not.Null);
            Assert.That(assemblyVersionElement!.Value, Is.EqualTo("1.2.3"));
        }
    }

    [Test]
    public void EnsureThat_SetAssemblyVersion_Updates_Existing_Version()
    {
        CsharpProject sut = CrateSut();
        sut.SetAssemblyVersion("1.2.3");
        sut.SetAssemblyVersion("2.3.4");

        var assemblyVersionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("AssemblyVersion");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(assemblyVersionElement, Is.Not.Null);
            Assert.That(assemblyVersionElement!.Value, Is.EqualTo("2.3.4"));
        }
    }

    [Test]
    public void EnsureThat_SetFileVersion_Works()
    {
        CsharpProject sut = CrateSut();
        sut.SetFileVersion("1.2.3");

        var fileVersionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("FileVersion");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(fileVersionElement, Is.Not.Null);
            Assert.That(fileVersionElement!.Value, Is.EqualTo("1.2.3"));
        }
    }

    [Test]
    public void EnsureThat_SetFileVersion_Updates_Existing_Version()
    {
        CsharpProject sut = CrateSut();
        sut.SetFileVersion("1.2.3");
        sut.SetFileVersion("2.3.4");

        var fileVersionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("FileVersion");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(fileVersionElement, Is.Not.Null);
            Assert.That(fileVersionElement!.Value, Is.EqualTo("2.3.4"));
        }
    }

    [Test]
    public void EnsureThat_SetLangVersion_KnwonValue_Works()
    {
        CsharpProject sut = CrateSut();
        sut.SetLangVersion(LangVersion.Preview);

        var langVersionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("LangVersion");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(langVersionElement, Is.Not.Null);
            Assert.That(langVersionElement!.Value, Is.EqualTo("preview"));
        }
    }

    [TestCase(1, 1, "1")]
    [TestCase(2, 1, "2")]
    [TestCase(6, 5, "6")]
    [TestCase(7, 3, "7.3")]
    [TestCase(8, 0, "8.0")]
    [TestCase(9, 0, "9.0")]
    public void EnsureThat_SetLangVersion_VersionNumber_Works(int major, int minor, string expected)
    {
        CsharpProject sut = CrateSut();
        sut.SetLangVersion(major, minor);

        var langVersionElement = XDocument.Parse(sut.XmlContent)
            .Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("LangVersion");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.WasModified, Is.True);
            Assert.That(langVersionElement, Is.Not.Null);
            Assert.That(langVersionElement!.Value, Is.EqualTo(expected));
        }
    }
}