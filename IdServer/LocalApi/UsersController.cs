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
    public UsersController(UserManager<ApplicationUser> userManager) {
      _userManager = userManager;
    }

    [HttpGet]
    public async Task<IList<ApplicationUser>> ListUsers() {
      var result = await _userManager.GetUsersInRoleAsync("BlazorClient1_User");
      return result;
    }

  }

}
