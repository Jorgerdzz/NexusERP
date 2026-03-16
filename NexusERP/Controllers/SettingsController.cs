using Microsoft.AspNetCore.Mvc;
using NexusERP.Extensions;
using Microsoft.AspNetCore.Authorization;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.Services;
using NexusERP.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
    public class SettingsController : Controller
    {
        private SettingsRepository repo;

        public SettingsController(SettingsRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            string idEmpresaString = HttpContext.User.FindFirstValue("EmpresaId");
            int idEmpresa = int.Parse(idEmpresaString);
            Empresa empresa = await this.repo.GetEmpresaAsync(idEmpresa);
            if (empresa == null) return NotFound();
            SettingsViewModel model = new SettingsViewModel
            {
                EmpresaId = empresa.Id,
                NombreComercial = empresa.NombreComercial,
                RazonSocial = empresa.RazonSocial,
                CIF = empresa.Cif,
                FechaAlta = empresa.FechaAlta?.ToString("dd MMM yyyy"),
                Activo = empresa.Activo
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmpresa(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                AlertService.Warning(TempData, "Por favor, revisa los datos introducidos.");
                return RedirectToAction("Index");
            }

            bool actualizado = await this.repo.UpdateEmpresaAsync(model.EmpresaId, model.NombreComercial, model.RazonSocial, model.CIF);

            if (actualizado)
            {
                AlertService.Toast(TempData, "Datos fiscales de la empresa actualizados", "success");
            }
            else
            {
                AlertService.Error(TempData, "No se pudo actualizar. Es posible que el CIF ya esté registrado.");
            }

            return RedirectToAction("Index");

        }

    }
}
