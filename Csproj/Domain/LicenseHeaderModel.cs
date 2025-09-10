namespace Csproj.Domain;

internal class LicenseHeaderModel
{
    public required string FileExtension { get; set; }
    public required string Content { get; set; }

    public const string LicenseTemplateFileName = ".licenseheader";
}
