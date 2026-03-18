using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Extensions;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.Models.UI;
using NexusERP.Repositories;
using NexusERP.Services;
using NexusERP.ViewModels;
using System.Security.Claims;
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
                ClaimsIdentity identity =
                    new ClaimsIdentity(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        ClaimTypes.Name, ClaimTypes.Role
                        );

                Claim claimName = new Claim(ClaimTypes.Name, resultado.user.Nombre);
                identity.AddClaim(claimName);

                Claim claimIniciales = new Claim("Iniciales", resultado.user.Nombre.ObtenerIniciales());
                identity.AddClaim(claimIniciales);

                Claim claimId = new Claim(ClaimTypes.NameIdentifier, resultado.user.Id.ToString());
                identity.AddClaim(claimId);

                string nombreRol = ((RolesUsuario)resultado.user.Rol).ToString();

                Claim claimRole = new Claim(ClaimTypes.Role, nombreRol);
                identity.AddClaim(claimRole);

                Claim claimEmail = new Claim(ClaimTypes.Email, resultado.user.Email);
                identity.AddClaim(claimEmail);

                Claim claimIdEmpresa = new Claim("EmpresaId", resultado.user.EmpresaId.ToString());
                identity.AddClaim(claimIdEmpresa);

                Claim claimIdEmpleado = new Claim("EmpleadoId", resultado.user.EmpleadoId.ToString());
                identity.AddClaim(claimIdEmpleado);

                Claim claimNombreEmpresa = new Claim("NombreEmpresa", resultado.user.Empresa.NombreComercial);
                identity.AddClaim(claimNombreEmpresa);

                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);


                if (userPrincipal.IsInRole("Admin"))
                {
                    AlertService.Toast(TempData, $"Bienvenido {claimName.Value}");
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    AlertService.Toast(TempData, $"Bienvenido {claimName.Value}");
                    return RedirectToAction("Index", "PortalEmpleado");
                }

            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return View(model);
            }

        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
