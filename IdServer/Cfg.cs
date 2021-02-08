using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using SharedLib;
using System.Collections.Generic;


namespace IdServer {

  public static class Cfg {

    public static string SubEnv { get; set; }

    public static void Init(IConfiguration Configuration) {
      SubEnv = Configuration["SubEnvironment"];

      ServicesUrls.BlazorClient1 = Configuration["ServicesUrls:BlazorClient1"];
      ServicesUrls.IdServer = Configuration["ServicesUrls:IdServer"];
      ServicesUrls.WebApi1 = Configuration["ServicesUrls:WebApi1"];

      IdentityServerCertificate.FileName = Configuration["IdentityServerCertificate:FileName"];
      IdentityServerCertificate.Password = Configuration["IdentityServerCertificate:Password"];
    }



    public const string CORSPolicy = "CORSPolicy";

    public static class IdentityServerCertificate {
      public static string FileName { get; set; }
      public static string Password { get; set; }
    }

    public static class ServicesUrls {
      public static string BlazorClient1 { get; set; }
      public static string IdServer { get; set; }
      public static string WebApi1 { get; set; }
    }
    public static class Policies {
      public const string UsersManagement_Add = "UsersManagement.Add_Policy";
      public const string UsersManagement_List = "UsersManagement.List_Policy";
      public const string CertificatesManagement_Create = "CertificatesManagement.Create_Policy";
    }





    public static class IdCfg {

      public static IEnumerable<IdentityResource> IdentityResources => new IdentityResource[] {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResources.Phone(),
        new IdentityResource{ 
          Name=IdNames.IdResources.Gender, 
          UserClaims= new List<string> {IdNames.Claims.IdServer_User_Gender},
        }
      };

      public static IEnumerable<ApiResource> ApiResources => new List<ApiResource> {
        // all the applications involved
        new ApiResource(IdNames.ApiRes.BlazorMSA) {
            Scopes = {
              IdNames.Scopes.WApi1WeatherRead,
              IdNames.Scopes.IdServerUsersRead,
              IdNames.Scopes.IdServerUsersWrite,
              IdNames.Scopes.IdServerCertificates
            },
        },
        new ApiResource(IdNames.ApiRes.WApi) {
            Scopes = {
              IdNames.Scopes.WApi1WeatherRead,
            },
        },
        new ApiResource(IdNames.ApiRes.IdServer) {
            Scopes = {
              IdNames.Scopes.IdServerUsersRead,
              IdNames.Scopes.IdServerUsersWrite,
              IdNames.Scopes.IdServerCertificates
            }
        }
      };

      public static IEnumerable<ApiScope> ApiScopes => new ApiScope[] {
        new ApiScope(){
          Name = IdNames.Scopes.WApi1WeatherRead,
          UserClaims = { IdNames.Claims.WApi1_Weather_GetById, IdNames.Claims.WApi1_Weather_List},
        },
        new ApiScope(){
          Name = IdNames.Scopes.IdServerUsersRead,
          UserClaims = { 
            IdNames.Claims.IdServer_Users_List,
          }
        },
        new ApiScope(){
          Name = IdNames.Scopes.IdServerUsersWrite,
          UserClaims = { 
            IdNames.Claims.IdServer_Users_Add
          }
        },
        new ApiScope(){
          Name = IdNames.Scopes.IdServerCertificates,
          UserClaims = { IdNames.Claims.IdServer_Cerificate_Create }
        }
      };

      public static IEnumerable<Client> Clients => new Client[] {

        // Clent Blazor #1
        new Client {
          ClientId = IdNames.Clients.BlazorClient1,
          AllowedGrantTypes = GrantTypes.Code,
          RequirePkce = true,
          RequireClientSecret = false,
          AllowedCorsOrigins = { Cfg.ServicesUrls.BlazorClient1 },
          AllowedScopes = {
            //IdentityResources
            IdNames.OidcStandardScopes.OpenId,
            IdNames.OidcStandardScopes.Profile,
            IdNames.OidcStandardScopes.Email,
            IdNames.OidcStandardScopes.Phone,
            IdNames.IdResources.Gender,
            //ApiScopes
            IdNames.Scopes.WApi1WeatherRead,
            IdNames.Scopes.IdServerUsersRead,
            IdNames.Scopes.IdServerUsersWrite,
            IdNames.Scopes.IdServerCertificates,
            },
          RedirectUris = { Cfg.ServicesUrls.BlazorClient1 + "/authentication/login-callback" },
          PostLogoutRedirectUris = { Cfg.ServicesUrls.BlazorClient1 + "/" },
          Enabled = true
        },

      };
    }
  }
}