using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using BlazorShortUrl.Data;

namespace BlazorShortUrl.Components.Account;

// This is a custom server-side AuthenticationStateProvider that revalidates the security stamp
// for the connected user every 30 minutes an interactive circuit is connected.
internal sealed class BlazorAuthStateProvider(ILoggerFactory loggerFactory) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    private readonly ILoggerFactory loggerFactory;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IOptions<IdentityOptions> options;
    
    // private readonly PersistentComponentState state;
    // private readonly PersistingComponentStateSubscription subscription;
    private Task<AuthenticationState>? authenticationStateTask;

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);
    
    public BlazorAuthStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> options) : this(loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        this.scopeFactory = scopeFactory;
        this.options = options;
        // Http.DefaultRequestHeaders.Authorization
    }
    
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        // Get the user manager from a new scope to ensure it fetches fresh data
        await using var scope = scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        return await ValidateSecurityStampAsync(userManager, authenticationState.User);
    }

    private async Task<bool> ValidateSecurityStampAsync(UserManager<AppUser> userManager,
        ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return false;
        }
        else if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }
        else
        {
            var principalStamp = principal.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType);
            var userStamp = await userManager.GetSecurityStampAsync(user);
            return principalStamp == userStamp;
        }
    }
}