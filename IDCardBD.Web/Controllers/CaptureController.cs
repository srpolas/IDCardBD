using Microsoft.AspNetCore.Mvc;

namespace IDCardBD.Web.Controllers
{
    public class CaptureController : Controller
    {
        public IActionResult Camera()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(string base64Image)
        {
            // Placeholder for server-side processing if needed
            // For now, we just acknowledge receipt
            return Json(new { success = true, message = "Image received" });
        }
    }
}
