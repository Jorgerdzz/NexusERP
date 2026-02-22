using Microsoft.AspNetCore.Mvc;

namespace NexusERP.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
