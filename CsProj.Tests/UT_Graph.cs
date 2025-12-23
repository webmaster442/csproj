using System;
using System.Collections.Generic;
using System.Text;

using CsProj.Core;

namespace CsProj.Tests;

[TestFixture]
internal class UT_DependencyTree
{
    private DependencyTree _sut;

    [SetUp]
    public void Setup()
    {
        CsharpProject[] projects =
        {
            new("C:\\App\\App.ApplicationServices\\App.ApplicationServices.csproj", Projects.App_ApplicationServices),
            new("C:\\App\\App.DomainModel\\App.DomainModel.csproj", Projects.App_DomainModel),
            new("C:\\App\\App.Ui\\App.Ui.csproj", Projects.App_UI),
            new("C:\\App\\Common.DomainModel\\Common.DomainModel.csproj", Projects.Common_DomainModel),
            new("C:\\App\\Common.DomainServices\\Common.DomainServices.csproj", Projects.Common_DomainServices),
            new("C:\\App\\Common.Ui\\Common.Ui.csproj", Projects.Common_Ui),
            new("C:\\App\\App.DomainServices\\App.DomainServices.csproj", Projects.App_DomainServices)
        };

        _sut = DependencyTree.CreateProjectTree(projects);
    }

    [Test]
    public void EnsureThat_ToNomnoml_Works()
    {
        string result = _sut.ToNomnoml();

        string expected =
            """
            #direction: down
            [Common.DomainServices] -> [App.DomainModel]
            [App.Ui] -> [App.ApplicationServices]
            [App.Ui] -> [App.DomainModel]
            [App.Ui] -> [App.DomainServices]
            [App.Ui] -> [Common.DomainModel]
            [App.Ui] -> [Common.DomainServices]
            [App.Ui] -> [Common.Ui]
            [App.DomainServices] -> [App.DomainModel]
            [App.DomainServices] -> [Common.DomainModel]
            [App.DomainModel] -> [Common.DomainModel]
            [App.ApplicationServices] -> [App.DomainModel]
            [App.ApplicationServices] -> [Common.DomainModel]
            [App.ApplicationServices] -> [Common.DomainServices]

            """;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void EnsureThat_ToMermaid_Works()
    {
        string result = _sut.ToMermaid();

        string expected =
            """
            graph TD
                Common.DomainServices --> App.DomainModel
                App.Ui --> App.ApplicationServices
                App.Ui --> App.DomainModel
                App.Ui --> App.DomainServices
                App.Ui --> Common.DomainModel
                App.Ui --> Common.DomainServices
                App.Ui --> Common.Ui
                App.DomainServices --> App.DomainModel
                App.DomainServices --> Common.DomainModel
                App.DomainModel --> Common.DomainModel
                App.ApplicationServices --> App.DomainModel
                App.ApplicationServices --> Common.DomainModel
                App.ApplicationServices --> Common.DomainServices

            """;

        Assert.That(result, Is.EqualTo(expected));
    }
}
