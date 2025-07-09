using Microsoft.AspNetCore.Identity;

namespace Uphbt.Identity;

public class User: IdentityUser<long>
{
    /// <summary>
    /// A unique identifier for public exposure, distinct from the internal database Id.
    /// Initialized with a new GUID by default
    /// </summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The full name of the user.
    /// Nullable, allowing for cases where it might not be immediately available.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// The date when the user was hired.
    /// Nullable, as not all users might have a hire date (e.g., external clients)
    /// or it might be unknown initially.
    /// </summary>
    public DateTime? DateHired { get; set; }

    public User() { }

    public User(string userName): base(userName) { }
}
