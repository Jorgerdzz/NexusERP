using Microsoft.AspNetCore.Mvc;

namespace NexusERP.Controllers
{
    public class DepartamentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
