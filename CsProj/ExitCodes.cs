namespace CsProj;

internal static class ExitCodes
{
    public const int Success = 0;
    public const int LoadError = 1;
    public const int GeneralError = 2;

    public const int Crash = byte.MaxValue;
}