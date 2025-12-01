namespace AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Specifications;

/// <summary>
/// Infrastructure initialization for registering handlers with Domain layer.
/// This should be called during application startup to register Infrastructure implementations.
/// </summary>
public static class InfrastructureInitialization
{
    /// <summary>
    /// Registers all Infrastructure handlers with Domain layer.
    /// Call this during application startup (e.g., in Program.cs or Startup.cs).
    /// </summary>
    public static void RegisterHandlers()
    {
        // No handlers to register - specifications are now pure domain (criteria only)
        // Includes, sorting, and pagination are handled at repository level
    }
}
