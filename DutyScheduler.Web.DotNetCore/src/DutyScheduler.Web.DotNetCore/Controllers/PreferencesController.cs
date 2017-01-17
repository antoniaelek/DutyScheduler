using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class PreferencesController : Controller
    {
        // GET: Home
		[HttpPost]
        public ActionResult Set()
        {
			return new JsonResult(new {
                Data = true
            });
        }
	}
}