
using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using NexusERP.Extensions;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    public class AccountController : Controller
    {
        private AccountRepository repo;

        public AccountController(AccountRepository repo)
        {
            this.repo = repo;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await this.repo.RegisterUserAsync(model);

            if (resultado.exito && resultado.usuarioCreado != null)
            {
                TempData["EXITO"] = resultado.mensaje;
                return RedirectToAction("LogIn");
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return View(model);
            }

        }

        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await this.repo.LogInUserAsync(model);
            if (resultado.acceso && resultado.user != null)
            {
                UsuarioSessionModel user = new UsuarioSessionModel
                {
                    IdUsuario = resultado.user.Id,
                    Nombre = resultado.user.Nombre,
                    Email = resultado.user.Email,
                    Rol = (RolesUsuario)resultado.user.Rol,
                    EmpresaId = resultado.user.EmpresaId,
                    NombreEmpresa = resultado.user.Empresa.NombreComercial
                };
                HttpContext.Session.SetObject("USUARIO_LOGUEADO", user);
                return RedirectToAction("Index", "Dashboard");

            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return View(model);
            }

        }

        public async Task<IActionResult> LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
