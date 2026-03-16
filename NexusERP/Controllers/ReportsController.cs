using Microsoft.AspNetCore.Mvc;
using NexusERP.DTOs;
using Microsoft.AspNetCore.Authorization;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
    public class ReportsController : Controller
    {
        private ReportsRepository repo;

        public ReportsController(ReportsRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index(int? anio)
        {
            int anioConsulta = anio ?? DateTime.Now.Year;

            ReportsViewModel model = new ReportsViewModel { AnioActual = anioConsulta };

            List<ReporteMensualDto> ingresos = await this.repo.GetIngresosPorMesAsync(anioConsulta);

            foreach (var item in ingresos)
            {
                model.IngresosMensuales[item.Mes - 1] = item.Total;
            }

            List<ReporteMensualDto> gastos = await this.repo.GetGastosPorMesAsync(anioConsulta);
            foreach (var item in gastos)
            {
                model.GastosMensuales[item.Mes - 1] = item.Total;
            }

            List<ReporteDepartamentoDto> costesDept = await this.repo.GetCostesPorDepartamentoAsync(anioConsulta);

            model.DepartamentosNombres = costesDept.Select(c => c.Departamento).ToList();
            model.GastosPorDepartamento = costesDept.Select(c => c.Total).ToList();

            return View(model);
        }
    }
}
