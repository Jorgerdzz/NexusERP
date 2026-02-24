using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Models;
using NexusERP.Repositories;
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

            var resultado = await this.repo.RegistrarCuentaAsync(model);

            if (resultado.exito && resultado.usuarioCreado != null)
            {
                TempData["EXITO"] = resultado.mensaje;
                await CrearCookieDeSesionAsync(resultado.usuarioCreado);
                return RedirectToAction("Index", "Dashboard");
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

            var resultado = await this.repo.ComprobarUsuarioAsync(model);
            if (resultado.acceso && resultado.user != null)
            {
                await CrearCookieDeSesionAsync(resultado.user, model.Recordarme);
                return RedirectToAction("Index", "Dashboard");

            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return View(model);
            }

        }

        public IActionResult LogOut()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task CrearCookieDeSesionAsync(Usuario user, bool recordar=false)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Id.ToString()),
                new Claim("EmpresaId", user.EmpresaId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = recordar
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

        }

    }
}
