using Microsoft.AspNetCore.Identity;

namespace BlazorShortUrl.Data
{
    public class AppRole : IdentityRole
    {
        public AppRole() { }

        public AppRole(string roleName) : base(roleName) { }

        public ICollection<IdentityRoleClaim<string>>? Claims { get; set; }
    }

}
