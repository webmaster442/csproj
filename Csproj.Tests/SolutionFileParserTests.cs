using Csproj.DomainServices;

namespace Csproj.Tests;

[TestFixture]
public class SolutionFileParserTests
{
    public const string XmlSolution = """
        <Solution>
          <Folder Name="/Solution Items/" />
          <Project Path="Csproj.Tests/Csproj.Tests.csproj" />
          <Project Path="Csproj/Csproj.csproj" />
        </Solution>
        """;

    public const string SolutionWithNotJustProjects = """
        Microsoft Visual Studio Solution File, Format Version 12.00
        # Visual Studio Version 17
        VisualStudioVersion = 17.0.31903.59
        MinimumVisualStudioVersion = 10.0.40219.1
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Csproj", "Csproj\Csproj.csproj", "{81D5BEFA-16A2-47F1-8F4F-215CE88F4CCD}"
        EndProject
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Csproj.Tests", "Csproj.Tests\Csproj.Tests.csproj", "{0C147F3F-B7A2-4686-8B11-925005A8A257}"
        EndProject
        Global
        	GlobalSection(SolutionConfigurationPlatforms) = preSolution
        		Debug|Any CPU = Debug|Any CPU
        		Release|Any CPU = Release|Any CPU
        	EndGlobalSection
        	GlobalSection(SolutionProperties) = preSolution
        		HideSolutionNode = FALSE
        	EndGlobalSection
        	GlobalSection(ProjectConfigurationPlatforms) = postSolution
        		{81D5BEFA-16A2-47F1-8F4F-215CE88F4CCD}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        		{81D5BEFA-16A2-47F1-8F4F-215CE88F4CCD}.Debug|Any CPU.Build.0 = Debug|Any CPU
        		{81D5BEFA-16A2-47F1-8F4F-215CE88F4CCD}.Release|Any CPU.ActiveCfg = Release|Any CPU
        		{81D5BEFA-16A2-47F1-8F4F-215CE88F4CCD}.Release|Any CPU.Build.0 = Release|Any CPU
        		{0C147F3F-B7A2-4686-8B11-925005A8A257}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        		{0C147F3F-B7A2-4686-8B11-925005A8A257}.Debug|Any CPU.Build.0 = Debug|Any CPU
        		{0C147F3F-B7A2-4686-8B11-925005A8A257}.Release|Any CPU.ActiveCfg = Release|Any CPU
        		{0C147F3F-B7A2-4686-8B11-925005A8A257}.Release|Any CPU.Build.0 = Release|Any CPU
        	EndGlobalSection
        EndGlobal
        """;

    public const string SampleSolution = """
        Microsoft Visual Studio Solution File, Format Version 12.00
        # Visual Studio Version 17
        VisualStudioVersion = 17.0.31903.59
        MinimumVisualStudioVersion = 10.0.40219.1
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject1", "TestProject1\TestProject1.csproj", "{548B2C08-FEB4-4EC4-A702-ACDF2BA492D7}"
        EndProject
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject2", "TestProject2\TestProject2.csproj", "{1B4A6065-BDF3-493C-B4A6-6C282689E2BF}"
        EndProject
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject3", "TestProject3\TestProject3.csproj", "{1A49F84C-CB92-44A1-9027-CC24629CF75A}"
        EndProject
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject4", "TestProject4\TestProject4.csproj", "{0146466C-7306-4822-8441-DF7F3181478B}"
        EndProject
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TestProject5", "..\TestProject5\TestProject5.csproj", "{EE52C562-3624-4FB7-857E-2199BC4F3DEE}"
        EndProject
        Global
        	GlobalSection(SolutionConfigurationPlatforms) = preSolution
        		Debug|Any CPU = Debug|Any CPU
        		Release|Any CPU = Release|Any CPU
        	EndGlobalSection
        	GlobalSection(SolutionProperties) = preSolution
        		HideSolutionNode = FALSE
        	EndGlobalSection
        	GlobalSection(ProjectConfigurationPlatforms) = postSolution
        		{548B2C08-FEB4-4EC4-A702-ACDF2BA492D7}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        		{548B2C08-FEB4-4EC4-A702-ACDF2BA492D7}.Debug|Any CPU.Build.0 = Debug|Any CPU
        		{548B2C08-FEB4-4EC4-A702-ACDF2BA492D7}.Release|Any CPU.ActiveCfg = Release|Any CPU
        		{548B2C08-FEB4-4EC4-A702-ACDF2BA492D7}.Release|Any CPU.Build.0 = Release|Any CPU
        		{1B4A6065-BDF3-493C-B4A6-6C282689E2BF}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        		{1B4A6065-BDF3-493C-B4A6-6C282689E2BF}.Debug|Any CPU.Build.0 = Debug|Any CPU
        		{1B4A6065-BDF3-493C-B4A6-6C282689E2BF}.Release|Any CPU.ActiveCfg = Release|Any CPU
        		{1B4A6065-BDF3-493C-B4A6-6C282689E2BF}.Release|Any CPU.Build.0 = Release|Any CPU
        		{1A49F84C-CB92-44A1-9027-CC24629CF75A}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        		{1A49F84C-CB92-44A1-9027-CC24629CF75A}.Debug|Any CPU.Build.0 = Debug|Any CPU
        		{1A49F84C-CB92-44A1-9027-CC24629CF75A}.Release|Any CPU.ActiveCfg = Release|Any CPU
        		{1A49F84C-CB92-44A1-9027-CC24629CF75A}.Release|Any CPU.Build.0 = Release|Any CPU
        		{0146466C-7306-4822-8441-DF7F3181478B}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        		{0146466C-7306-4822-8441-DF7F3181478B}.Debug|Any CPU.Build.0 = Debug|Any CPU
        		{0146466C-7306-4822-8441-DF7F3181478B}.Release|Any CPU.ActiveCfg = Release|Any CPU
        		{0146466C-7306-4822-8441-DF7F3181478B}.Release|Any CPU.Build.0 = Release|Any CPU
        		{EE52C562-3624-4FB7-857E-2199BC4F3DEE}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        		{EE52C562-3624-4FB7-857E-2199BC4F3DEE}.Debug|Any CPU.Build.0 = Debug|Any CPU
        		{EE52C562-3624-4FB7-857E-2199BC4F3DEE}.Release|Any CPU.ActiveCfg = Release|Any CPU
        		{EE52C562-3624-4FB7-857E-2199BC4F3DEE}.Release|Any CPU.Build.0 = Release|Any CPU
        	EndGlobalSection
        EndGlobal
        """;

    [Test]
    public void EnsureThat_GetProjects_ReturnsExpected_InSampleSolution()
    {
        using StringReader _solutuionFile = new StringReader(SampleSolution);

        string[] expected =
        {
            @"c:\test\TestProject1\TestProject1.csproj",
            @"c:\test\TestProject2\TestProject2.csproj",
            @"c:\test\TestProject3\TestProject3.csproj",
            @"c:\test\TestProject4\TestProject4.csproj",
            @"c:\TestProject5\TestProject5.csproj"
        };
        var results = SolutionFileParser.GetProjects(_solutuionFile, ".csproj", @"c:\test").ToArray();

        Assert.That(results, Is.EquivalentTo(expected));
    }

    [Test]
    public void EnsureThat_GetProjects_ReturnsExpected_InSolutionWithNotJustProjects()
    {
        using StringReader _solutuionFile = new StringReader(SolutionWithNotJustProjects);

        string[] expected =
        {
            @"c:\test\Csproj\Csproj.csproj",
            @"c:\test\Csproj.Tests\Csproj.Tests.csproj"
        };
        var results = SolutionFileParser.GetProjects(_solutuionFile, ".csproj", @"c:\test").ToArray();

        Assert.That(results, Is.EquivalentTo(expected));
    }

    [Test]
    public void EnsureThat_GetProjects_ReturnsExpected_InSolutionWithXml()
    {
        using StringReader _solutuionFile = new StringReader(XmlSolution);

        string[] expected =
        {
            @"c:\test\Csproj\Csproj.csproj",
            @"c:\test\Csproj.Tests\Csproj.Tests.csproj"
        };
        var results = SolutionFileParser.GetProjects(_solutuionFile, ".csproj", @"c:\test").ToArray();

        Assert.That(results, Is.EquivalentTo(expected));
    }
}
