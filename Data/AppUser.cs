using Microsoft.AspNetCore.Identity;

namespace BlazorShortUrl.Data
{
    // Add profile data for application users by adding properties to the AppUser class
    public class ApplicationUser : IdentityUser
    {
        public ICollection<IdentityUserRole<string>>? Roles { get; set; }
        public ICollection<IdentityUserClaim<string>>? Claims { get; set; }
    }
}
