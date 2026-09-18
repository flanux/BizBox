using Microsoft.AspNetCore.Identity;

namespace Bizbox.Models;

// Extends Identity's built-in user. Add project-specific fields here later
// (e.g. a display name) rather than creating a separate profile table for now.
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
