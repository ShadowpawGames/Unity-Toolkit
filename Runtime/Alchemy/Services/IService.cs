using System.Threading.Tasks;

namespace Shadowpaw.Alchemy {
  /// <summary>
  /// Interface for Services in the Alchemy framework. <br />
  /// Services provide global project-wide functionality.
  /// </summary>
  /// <remarks>
  /// - Services are automatically created by the Alchemy Engine. <br />
  /// - Services may only have a single non-default constructor. <br />
  /// - Dependencies will be injected via constructor parameters.<br />
  /// - Dependencies will be injected via the [Inject] attribute.<br /> 
  /// - Constructor parameters with a default value are considered optional.<br />
  /// - Services may depend on other services, provided deps are non-cyclic.<br />
  /// - Services may depend on Components, which will be created at runtime.<br />
  /// - Services that are Unity Components will be attached to a GameObject.<br />
  /// </remarks>
  public interface IService {
    /// <summary>
    /// Called by the Engine when the service is initialized. <br />
    /// Dependencies will be injected before this method is called.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> which completes when the service is initialized. <br />
    /// Return Task.CompletedTask if no async work is required.
    /// </returns>
    Task OnServiceInit();

    /// <summary>
    /// Called by the Engine when the service is reset. <br />
    /// This should return the Service to its initial state.
    /// </summary>
    void OnServiceReset();

    /// <summary>
    /// Called by the Engine when the service is destroyed. <br />
    /// This should clean up any resources used by the service.
    /// </summary>
    void OnServiceDestroy();
  }
}
