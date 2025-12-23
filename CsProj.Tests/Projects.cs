namespace CsProj.Tests;

internal class Projects
{
    public const string App_ApplicationServices =
        """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

          <ItemGroup>
            <ProjectReference Include="..\App.DomainModel\App.DomainModel.csproj" />
            <ProjectReference Include="..\Common.DomainModel\Common.DomainModel.csproj" />
            <ProjectReference Include="..\Common.DomainServices\Common.DomainServices.csproj" />
          </ItemGroup>

        </Project>
        """;

    public const string App_DomainModel =
        """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

          <ItemGroup>
            <ProjectReference Include="..\Common.DomainModel\Common.DomainModel.csproj" />
          </ItemGroup>

        </Project>
        """;

    public const string App_DomainServices =
        """
        <Project Sdk="Microsoft.NET.Sdk">
        
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

          <ItemGroup>
            <ProjectReference Include="..\App.DomainModel\App.DomainModel.csproj" />
            <ProjectReference Include="..\Common.DomainModel\Common.DomainModel.csproj" />
          </ItemGroup>

        </Project>
        """;

    public const string App_UI =
        """
        <Project Sdk="Microsoft.NET.Sdk">
        

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

          <ItemGroup>
            <ProjectReference Include="..\App.ApplicationServices\App.ApplicationServices.csproj" />
            <ProjectReference Include="..\App.DomainModel\App.DomainModel.csproj" />
            <ProjectReference Include="..\App.DomainServices\App.DomainServices.csproj" />
            <ProjectReference Include="..\Common.DomainModel\Common.DomainModel.csproj" />
            <ProjectReference Include="..\Common.DomainServices\Common.DomainServices.csproj" />
            <ProjectReference Include="..\Common.Ui\Common.Ui.csproj" />
          </ItemGroup>

        </Project>
        """;

    public const string Common_DomainModel =
        """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

        </Project>
        
        """;

    public const string Common_DomainServices =
        """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

          <ItemGroup>
            <ProjectReference Include="..\App.DomainModel\App.DomainModel.csproj" />
          </ItemGroup>

        </Project>
        
        """;

    public const string Common_Ui =
        """
        <Project Sdk="Microsoft.NET.Sdk">
        

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

        </Project>
        """;
}
