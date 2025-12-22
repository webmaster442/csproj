namespace CsProj.Core;

internal interface IReadonlyCsharpProject
{
    string AbsolutePath { get; }
    bool IsSdkStyleProject { get; }
    string XmlContent { get; }
}