namespace CsProj.Core;

internal sealed class PackageReference : IEquatable<PackageReference?>
{
    public string PackageName { get; }
    public Version? Version { get; }

    public PackageReference(string packageName, Version? version = null)
    {
        PackageName = packageName;
        Version = version;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PackageReference);
    }

    public bool Equals(PackageReference? other)
    {
        return other is not null &&
               PackageName == other.PackageName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PackageName);
    }
}
