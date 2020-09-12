using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lexikar.Controllers
{
    [Route("Default")]
    public class DefaultController : Controller
    {
        [Route("")]
        [Route("/")]
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
