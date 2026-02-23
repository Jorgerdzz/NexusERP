using Microsoft.AspNetCore.Mvc;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
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

            if (resultado.exito)
            {
                TempData["EXITO"] = resultado.mensaje;
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
        public IActionResult LogIn(Usuario user)
        {
            return View();
        }

        public IActionResult LogOut()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
