using IdServer.Data;
using IdServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace IdServer.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  [Authorize(Policy = "UsersManagementPolicy")]
  public class UsersController : ControllerBase {
    private UserManager<ApplicationUser> _userManager;
    private RoleManager<IdentityRole> _roleManager;
    private ApplicationDbContext _appDbContext;
    public UsersController(UserManager<ApplicationUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          ApplicationDbContext appDbContext
                          ) {
      _userManager = userManager;
      _roleManager = roleManager;
      _appDbContext = appDbContext;
    }


    [HttpGet]
    public async Task<IList<ApplicationUser>> ListUsers() {
      var result = await _userManager.GetUsersInRoleAsync("BlazorClient1_User");
      return result;
    }



    [HttpPost]
    public async Task<ApplicationUser> AddUser(ApplicationUser user) {
      var usr = new ApplicationUser() { UserName = user.UserName, Email = user.Email };
      using (var transaction = await _appDbContext.Database.BeginTransactionAsync()) {
        try {
          // Add the User
          var result = await _userManager.CreateAsync(usr, "Pass123$");
          if (!result.Succeeded) throw new Exception();
          // Assign le role "BlazorClient1_User" to the created user
          var resultRole = await _userManager.AddToRoleAsync(usr, "BlazorClient1_User");
          if (!resultRole.Succeeded) throw new Exception();
          // Commit changes to DB
          await transaction.CommitAsync();
        }
        catch {
          await transaction.RollbackAsync();
          usr = null;
        }
      }
      return usr;
    }


  }

}
