using Microsoft.AspNetCore.Mvc;

namespace NexusERP.Controllers
{
    public class AccountingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
