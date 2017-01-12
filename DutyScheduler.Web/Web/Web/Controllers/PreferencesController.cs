using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class PreferencesController : Controller
    {
        // GET: Home
		[HttpPost]
        public ActionResult Set()
        {
			return new JsonResult() {
				Data = true
			};
        }
	}
}