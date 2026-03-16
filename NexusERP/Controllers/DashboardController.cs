using Microsoft.AspNetCore.Mvc;
using NexusERP.DTOs;
using NexusERP.Extensions;
using Microsoft.AspNetCore.Authorization;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
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

            string nombreUsuario = HttpContext.User.FindFirstValue(ClaimTypes.Name);

            HomeDashboardViewModel model = new HomeDashboardViewModel
            {
                NombreUsuario = nombreUsuario,
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
