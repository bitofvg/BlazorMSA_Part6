using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLib;

namespace Host.Quickstart.Account {

  //https://identityserver4.readthedocs.io/en/latest/topics/windows.html?highlight=windows
  [Authorize(AuthenticationSchemes = IdNames.AuthSchemes.Negotiate)]
  public class NegotiateController : Controller {
    [HttpGet]
    public async Task<IActionResult> Challenge(string returnUrl) {
      return await ChallengeWindowsAsync(returnUrl);
    }

    private async Task<IActionResult> ChallengeWindowsAsync(string returnUrl) {
      var wp = HttpContext.User;
      // we will issue the external cookie and then redirect the
      // user back to the external callback, in essence, treating windows
      // auth the same as any other external authentication mechanism
      var props = new AuthenticationProperties() {
          RedirectUri = Url.Action("Callback", "External"),
          Items =
            {
                { "returnUrl", returnUrl },
                { "scheme", IdNames.AuthSchemes.Negotiate },
            }
        };

        var id = new ClaimsIdentity(IdNames.AuthSchemes.Negotiate);

        // the sid is a good sub value
        id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.FindFirst(ClaimTypes.PrimarySid).Value));

        // the account name is the closest we have to a display name
        id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

        // add the groups as claims -- be careful if the number of groups is too large
        var wi = wp.Identity as WindowsIdentity;

        // translate group SIDs to display names
        var groups = wi.Groups.Translate(typeof(NTAccount));
        var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
        id.AddClaims(roles);


        await HttpContext.SignInAsync(
            IdentityServerConstants.ExternalCookieAuthenticationScheme,
            new ClaimsPrincipal(id),
            props);
        return Redirect(props.RedirectUri);

    }

  }
}
