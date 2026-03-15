using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Extensions;
using NexusERP.Filters;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.Services;
using NexusERP.ViewModels;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [AuthorizeUser(Rol = RolesUsuario.Admin)]
    public class SettingsController : Controller
    {
        private SettingsRepository repo;

        public SettingsController(SettingsRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            UsuarioSessionModel user = HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");
            Empresa empresa = await this.repo.GetEmpresaAsync(user.EmpresaId);
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
