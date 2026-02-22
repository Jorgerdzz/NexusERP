using Microsoft.AspNetCore.Mvc;

namespace NexusERP.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
