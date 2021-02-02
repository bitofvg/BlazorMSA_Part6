using IdServer.Data;
using IdServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Transactions;
using static IdServer.SelfSignedServerCertificate;


//doc: https://bitofvg.wordpress.com/2021/01/29/identity-server-4-self-signed-certificates/

namespace IdServer.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  [Authorize(Policy = "UsersManagementPolicy")]
  public class CertsController : ControllerBase {

    [HttpPost]
    public IActionResult NewCertificate(NewCertificate cert) {
      var ssc = SelfSignedServerCertificate.Create(cert);
      return File(ssc, "application/octet-stream");
    }

  }
}
