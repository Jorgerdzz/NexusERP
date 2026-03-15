using Microsoft.AspNetCore.Mvc;
using NexusERP.Extensions;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.Services;
using NexusERP.ViewModels;

namespace NexusERP.Controllers
{
    public class ProfileController : Controller
    {
        private ProfileRepository repo;

        public ProfileController(ProfileRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            UsuarioSessionModel usuarioActual = HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");
            Usuario user = await this.repo.GetPerfilUsuarioAsync(usuarioActual.IdUsuario);
            if (user == null) return NotFound();
            MiPerfilViewModel model = new MiPerfilViewModel
            {
                UsuarioId = user.Id,
                NombreUsuario = user.Nombre,
                Email = user.Email,
                NombreEmpresa = user.Empresa?.NombreComercial,
                EstaVinculadoAEmpleado = user.EmpleadoId.HasValue
            };
            if(model.EstaVinculadoAEmpleado && user.Empleado != null)
            {
                model.EmpleadoNombreCompleto = $"{user.Empleado.Nombre} {user.Empleado.Apellidos}";
                model.EmpleadoDNI = user.Empleado.Dni;
                model.EmpleadoTelefono = user.Empleado.Telefono;
                model.DepartamentoNombre = user.Empleado.Departamento?.Nombre ?? "Sin asignar";
                model.FechaAntiguedad = user.Empleado.FechaAntiguedad.ToString("dd MM yyyy");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfo(MiPerfilViewModel model)
        {
            ModelState.Remove("PasswordActual");
            ModelState.Remove("NuevaPassword");
            ModelState.Remove("ConfirmarPassword");

            if (!ModelState.IsValid)
            {
                AlertService.Warning(TempData, "Revisa los datos introducidos.");
                return RedirectToAction("Index");
            }

            bool actualizado = await this.repo.UpdatePerfilUsuarioAsync(model.UsuarioId, model.NombreUsuario, model.Email);

            if (actualizado)
            {
                UsuarioSessionModel usuarioActual = HttpContext.Session.GetObject<UsuarioSessionModel>("USUARIO_LOGUEADO");
                usuarioActual.Nombre = model.NombreEmpresa;
                usuarioActual.Email = model.Email;
                HttpContext.Session.SetObject("USUARIO_LOGUEADO", usuarioActual);
                AlertService.Toast(TempData, "Perfil actualizado correctamente", "success");
            }
            else
            {
                AlertService.Error(TempData, "Hubo un error al guardar");
            }
            return RedirectToAction("Index");
        }
    }
}
