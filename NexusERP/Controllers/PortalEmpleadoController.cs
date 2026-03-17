using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Extensions;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.Security.Claims;

namespace NexusERP.Controllers
{
    public class PortalEmpleadoController : Controller
    {
        private PayrollRepository repoNominas;

        public PortalEmpleadoController(PayrollRepository repoNominas)
        {
            this.repoNominas = repoNominas;
        }

        [Authorize(Policy = "EMPLEADO")]
        public IActionResult Index()
        {
            string nombreUsuario = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            ViewBag.NombreEmpleado = nombreUsuario;
            return View();
        }

        [Authorize(Policy = "DESCARGARPDF")]
        public async Task<IActionResult> MisNominas(int? mes, int? anio, int? empleadoId)
        {
            int idEmpleadoFinal;

            if (User.IsInRole(RolesUsuario.Admin.ToString()) && empleadoId.HasValue)
            {
                idEmpleadoFinal = empleadoId.Value;
                ViewBag.EmpleadoId = empleadoId.Value;
            }
            else
            {
                string idEmpleadoString = HttpContext.User.FindFirstValue("EmpleadoId");
                idEmpleadoFinal = int.Parse(idEmpleadoString);
            }

            int mesConsulta = mes ?? DateTime.Now.Month;
            int anioConsulta = anio ?? DateTime.Now.Year;
            Nomina nomina = await this.repoNominas.GetNominaEmpleadoPorMesAsync(idEmpleadoFinal, mesConsulta, anioConsulta);
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
