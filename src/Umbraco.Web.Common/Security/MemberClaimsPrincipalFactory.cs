using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     A <see cref="UserClaimsPrincipalFactory{TUser}"/> for members
/// </summary>
public class MemberClaimsPrincipalFactory : UserClaimsPrincipalFactory<MemberIdentityUser>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeClaimsPrincipalFactory" /> class.
    /// </summary>
    /// <param name="userManager">The user manager</param>
    /// <param name="optionsAccessor">The <see cref="BackOfficeIdentityOptions" /></param>
    public MemberClaimsPrincipalFactory(
        UserManager<MemberIdentityUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
    {
    }

    protected virtual string AuthenticationType => IdentityConstants.ApplicationScheme;

    /// <inheritdoc />
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(MemberIdentityUser member)
    {
        // Get the base
        ClaimsIdentity baseIdentity = await base.GenerateClaimsAsync(member);

        // now create a new one with the correct authentication type

        // NOTE: NameClaim is not Options.ClaimsIdentity.UserNameClaimType
        // As we override the default from MS Identity to not be ClaimType.Name
        // and be 'Umbraco.MemberUserName' to store the login/username warren@umbraco.com
        // Then we can use the name claim type to store actual friendly member name
        var memberIdentity = new ClaimsIdentity(
            AuthenticationType,
            ClaimTypes.Name,
            Options.ClaimsIdentity.RoleClaimType);

        // Explicitly set the Name claim to be the actual friendly member name
        // and not the login/email of the member
        // This means the identity above we are creating will assign User.Identity.Name correctly
        memberIdentity.AddOrUpdateClaim(new Claim(ClaimTypes.Name, member.Name ?? "Unknown"));

        // and merge all others from the base implementation
        memberIdentity.MergeAllClaims(baseIdentity);

        // And merge claims added to the user, for instance in OnExternalLogin, we need to do this explicitly, since the claims are IdentityClaims, so it's not handled by memberIdentity.
        foreach (Claim claim in member.Claims
                     .Where(claim => claim.ClaimType is not null && claim.ClaimValue is not null)
                     .Where(claim => memberIdentity.HasClaim(claim.ClaimType!, claim.ClaimValue!) is false)
                     .Select(x => new Claim(x.ClaimType!, x.ClaimValue!)))
        {
            memberIdentity.AddClaim(claim);
        }

        return memberIdentity;
    }
}
