using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorClient1 {
  public class CustomUserFactory : AccountClaimsPrincipalFactory<RemoteUserAccount> {
    public CustomUserFactory(IAccessTokenProviderAccessor accessor) : base(accessor) {
    }

    public async override ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options) {
      var user = await base.CreateUserAsync(account, options);

      if (user.Identity.IsAuthenticated) {
        var identity = (ClaimsIdentity)user.Identity;
        // delete all the role claims
        var roleClaims = identity.FindAll(identity.RoleClaimType).ToArray();
        if (roleClaims != null && roleClaims.Any()) {
          foreach (var existingClaim in roleClaims) {
            identity.RemoveClaim(existingClaim);
          }
          // load the claim (role: "Admin" or role: [ "User, "Admin,..] an array )
          // (set in Programs.cs: options.UserOptions.RoleClaim = "role";)
          var rolesElem = account.AdditionalProperties[identity.RoleClaimType];
          if (rolesElem is JsonElement roles) {
            if (roles.ValueKind == JsonValueKind.Array) {
              // loop the role array
              foreach (var role in roles.EnumerateArray()) {
                //add the role as claim type: ClaimsType.Role 
                identity.AddClaim(new Claim(options.RoleClaim, role.GetString()));
              }
            }
            else { //is not an array, load it directly
              identity.AddClaim(new Claim(options.RoleClaim, roles.GetString()));
            }
          }
        }
      }

      return user;
    }
  }

}
