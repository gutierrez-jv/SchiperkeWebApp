namespace SchiperkeWebApp.Services;

internal static class PatientNumberCoordinator
{
    internal static readonly SemaphoreSlim Lock = new(1, 1);
}
