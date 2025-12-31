using Microsoft.AspNetCore.Mvc;

namespace medicare_pvt.Controllers
{
    public class RandomController : Controller
    {
        public IActionResult MedicalRecord()
        {
            return View();
        }

        // (also include Feedback and Payment if used)
        public IActionResult Feedback()
        {
            return View();
        }

        public IActionResult Payment()
        {
            return View();
        }
    }
}
