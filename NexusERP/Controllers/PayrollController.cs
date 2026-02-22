using Microsoft.AspNetCore.Mvc;

namespace NexusERP.Controllers
{
    public class PayrollController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
