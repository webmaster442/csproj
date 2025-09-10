using Csproj.DomainServices;

namespace Csproj.Tests;

[TestFixture]
public class LicenseHeaderParserTests
{
    [Test]
    public void EnsureThat_Parse_ReturnsExpectedResult()
    {
        const string testData = """
            ﻿extensions: .cs, .csx
            //-----------------------------------------------------------------------------
            // (c) 2019-2025 Ruzsinszki Gábor
            // This code is licensed under MIT license (see LICENSE for details)
            //-----------------------------------------------------------------------------

            extensions: .css
            /*-----------------------------------------------------------------------------
             (c) 2019-2025 Ruzsinszki Gábor
             This code is licensed under MIT license (see LICENSE for details)
            -----------------------------------------------------------------------------*/
            """;

        Dictionary<string, string> result = LicenseHeaderParser.Parse(testData);

        Assert.Multiple(() =>
        {
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result["﻿.cs"], Does.Contain("// (c) 2019-2025 Ruzsinszki Gábor"));
            Assert.That(result["﻿.cs"], Does.Contain("// This code is licensed under MIT license (see LICENSE for details)"));
            
            Assert.That(result[".csx"], Does.Contain("// (c) 2019-2025 Ruzsinszki Gábor"));
            Assert.That(result[".csx"], Does.Contain("// This code is licensed under MIT license (see LICENSE for details)"));

            Assert.That(result[".css"], Does.Contain("/*"));
            Assert.That(result[".css"], Does.Contain(" (c) 2019-2025 Ruzsinszki Gábor"));
            Assert.That(result[".css"], Does.Contain(" This code is licensed under MIT license (see LICENSE for details)"));
            Assert.That(result[".css"], Does.Contain("*/"));
        });
    }
}
