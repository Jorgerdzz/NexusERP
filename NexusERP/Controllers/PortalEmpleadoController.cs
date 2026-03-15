using Microsoft.AspNetCore.Mvc;
using NexusERP.Extensions;
using NexusERP.Models;

namespace NexusERP.Controllers
{
    public class PortalEmpleadoController : Controller
    {
         
        public IActionResult Index()
        {
            UsuarioSessionModel usuarioActual = HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");
            ViewBag.NombreEmpleado = usuarioActual.Nombre;
            return View();
        }
    }
}
