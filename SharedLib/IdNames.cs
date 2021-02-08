using System;

namespace SharedLib {
  using System;

  public static class IdNames {

    public static class OidcStandardScopes {
      public const string OpenId = "openid";
      public const string Profile = "profile";
      public const string Email = "email";
      public const string Phone = "phone";
    }


    public static class Clients {
      public const string BlazorClient1 = "BlazorClient1";
    }


    public static class IdResources {
      public const string Gender = "Gender_IdentityResource";
    }

    public static class ApiRes {
      public const string WApi = "WApi1_ApiResource";
      public const string IdServer = "IdServer_ApiResource";
      public const string BlazorMSA = "BlazorMSA_ApiResource";
    }

    public static class Scopes {
      public const string WApi1WeatherRead = "WApi1.Weather.Read_Scope";
      public const string IdServerUsersRead = "IdServer.Users.Read_Scope";
      public const string IdServerUsersWrite = "IdServer.Users.Write_Scope";
      public const string IdServerCertificates = "IdServer.Certificates_Scope";
    }


    // This Roles are stord in the DB (table AspNetRoles)
    public static class Roles {
      public const string IdServer_Admin = "IdServer.Admin_Role";
      public const string IdServer_User = "IdServer.User_Role";
      public const string BlazorClient1_Admin = "BlazorClient1.Admin_Role";
      public const string BlazorClient1_User = "BlazorClient1.User_Role";
    }



    // This Claims are stord in the DB (table AspNetUsersClaims or AspNetRoleClaims) 
    // in addition to the Standars Claims (name, given_name, websie, phonenumber, phonenumber_verified,...)
    public static class Claims {
      public const string IdServer_User_Gender = "IdServer.User.Gender_Claim";
      public const string IdServer_Users_List = "IdServer.Users.List_Claim";
      public const string IdServer_Users_Add = "IdServer.Users.Add_Claim";
      public const string IdServer_Cerificate_Create = "IdServer.Cerificate.Create_Claim";
      public const string WApi1_Weather_GetById = "WApi1.Weather.GetById_Claim";
      public const string WApi1_Weather_List = "WApi1.Weather.List_Claim";
    }




  }

}
