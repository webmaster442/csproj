namespace CsProj.Core;

internal class Statistics
{
    public int NotModified { get; set; }
    public int Modified { get; set; }
    public int Skipped { get; set; }

    public int Total => NotModified + Modified + Skipped;
}
