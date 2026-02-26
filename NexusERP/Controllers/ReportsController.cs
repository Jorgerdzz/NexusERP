using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Filters;

namespace NexusERP.Controllers
{
    [AuthorizeUser(Rol = RolesUsuario.Admin)]
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
