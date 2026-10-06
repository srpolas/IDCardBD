using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCardBD.Web.Controllers
{
    [Authorize]
    public class CaptureController : Controller
    {
        public IActionResult Camera()
        {
            return View();
        }
    }
}
