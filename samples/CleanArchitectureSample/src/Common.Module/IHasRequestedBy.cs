namespace Common.Module;

/// <summary>
/// Marker interface for messages that carry information about who initiated the request.
/// An endpoint filter automatically populates the <see cref="RequestedBy"/> property
/// from the HTTP context so that handlers receive the caller's identity without
/// coupling to ASP.NET Core.
/// </summary>
public interface IHasRequestedBy
{
    /// <summary>
    /// Gets or sets the identity of the user or system that initiated the request.
    /// Populated automatically by <c>SetRequestedByFilter</c> when the message
    /// is received through a minimal API endpoint.
    /// </summary>
    string? RequestedBy { get; set; }
}
