using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdServer.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdServer {


  public class ProfileService : IProfileService {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;

    public ProfileService(
      UserManager<ApplicationUser> userManager,
      RoleManager<IdentityRole> roleManager,
      IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) {
      _userManager = userManager;
      _roleManager = roleManager;
      _claimsFactory = claimsFactory;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context) {

      //https://medium.com/@ffimnsr/adding-identity-roles-to-identity-server-4-in-net-core-3-1-d42b64ff6675
      // Get the user 
      var user = await _userManager.GetUserAsync(context.Subject);
      // Get the ClainsPrincipal from the user
      var principal = await _claimsFactory.CreateAsync(user);
      // All the user claims + roles (claims "role") + role claims
      var nameClaims = principal.Claims.ToList();
      // Get requested claim types by the client
      var reqclaims = context.RequestedClaimTypes;
      // Filter out only the requested claims
      var retclaims = nameClaims.Where(claim => reqclaims.Contains(claim.Type)).ToList();
      // Add roles
      retclaims.AddRange(nameClaims.Where(claim => claim.Type=="role").ToList());

      if (_userManager.SupportsUserRole) {
        // Get roles list
        IList<string> roles = await _userManager.GetRolesAsync(user);
        foreach (var rolename in roles) {
          if (_roleManager.SupportsRoleClaims) {
            // Get role
            IdentityRole role = await _roleManager.FindByNameAsync(rolename);
            if (role != null) {
              // Get role claims
              var roleclaims = await _roleManager.GetClaimsAsync(role);
              // Filter user role principalclaims
              var userRoleClaims = nameClaims.Where(claim => roleclaims.Where(rc => rc.Type == claim.Type && rc.Value == claim.Value).Any()).ToList();
              // Add user role claims
              retclaims.AddRange(userRoleClaims);
            }
          }
        }
      }

      //return only the requested claims + roles 
      context.IssuedClaims.AddRange(retclaims);
    }

    public async Task IsActiveAsync(IsActiveContext context) {
      var user = await _userManager.GetUserAsync(context.Subject);
      context.IsActive = (user != null) && user.LockoutEnabled;
    }


  }



}
