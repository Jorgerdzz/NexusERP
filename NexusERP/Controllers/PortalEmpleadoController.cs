using Microsoft.AspNetCore.Mvc;
using NexusERP.Extensions;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;

namespace NexusERP.Controllers
{
    public class PortalEmpleadoController : Controller
    {
        private PayrollRepository repoNominas;

        public PortalEmpleadoController(PayrollRepository repoNominas)
        {
            this.repoNominas = repoNominas;
        }

        public IActionResult Index()
        {
            UsuarioSessionModel usuarioActual = HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");
            ViewBag.NombreEmpleado = usuarioActual.Nombre;
            return View();
        }

        public async Task<IActionResult> MisNominas(int? mes, int? anio)
        {
            UsuarioSessionModel usuarioActual = HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");
            int mesConsulta = mes ?? DateTime.Now.Month;
            int anioConsulta = anio ?? DateTime.Now.Year;
            Nomina nomina = await this.repoNominas.GetNominaEmpleadoPorMesAsync(usuarioActual.EmpleadoId, mesConsulta, anioConsulta);
            MisNominasViewModel model = new MisNominasViewModel
            {
                MesSeleccionado = mesConsulta,
                AnoSeleccionado = anioConsulta,
                NominaActual = nomina
            };
            return View(model);
        }
    }
}
