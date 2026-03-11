using Microsoft.AspNetCore.Mvc;
using NexusERP.DTOs;
using NexusERP.Enums;
using NexusERP.Extensions;
using NexusERP.Filters;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [AuthorizeUser(Rol = RolesUsuario.Admin)]
    public class DashboardController : Controller
    {
        private DashboardRepository repo;

        public DashboardController(DashboardRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            int anioActual = DateTime.Now.Year;

            DashboardMetricsDto estadisticas = await this.repo.GetEstadisticasAsync(anioActual);

            UsuarioSessionModel user = HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");

            HomeDashboardViewModel model = new HomeDashboardViewModel
            {
                NombreUsuario = user.Nombre,
                TotalFacturadoAnual = estadisticas.TotalFacturadoAnual,
                TotalGastoSalarial = estadisticas.TotalGastoSalarial,
                FacturasPendientes = estadisticas.FacturasPendientes,
                TieneDepartamentos = estadisticas.TieneDepartamentos,
                TieneClientes = estadisticas.TieneClientes,
                TieneEmpleados = estadisticas.TieneEmpleados
            };

            return View(model);
        }
    }
}
