using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Extensions;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.Security.Claims;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "EMPLEADO")]
    public class PortalEmpleadoController : Controller
    {
        private PayrollRepository repoNominas;

        public PortalEmpleadoController(PayrollRepository repoNominas)
        {
            this.repoNominas = repoNominas;
        }

        public IActionResult Index()
        {
            string nombreUsuario = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            ViewBag.NombreEmpleado = nombreUsuario;
            return View();
        }

        public async Task<IActionResult> MisNominas(int? mes, int? anio)
        {
            string idEmpleadoString = HttpContext.User.FindFirstValue("EmpleadoId");
            int idEmpleado = int.Parse(idEmpleadoString);
            int mesConsulta = mes ?? DateTime.Now.Month;
            int anioConsulta = anio ?? DateTime.Now.Year;
            Nomina nomina = await this.repoNominas.GetNominaEmpleadoPorMesAsync(idEmpleado, mesConsulta, anioConsulta);
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
