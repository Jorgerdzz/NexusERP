using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Filters;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.Repositories;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [AuthorizeUser(Rol = RolesUsuario.Admin)]
    public class AccountingController : Controller
    {
        private AccountingRepository repo;
        private HelperSessionContextAccessor contextAccessor;

        public AccountingController(AccountingRepository repo, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
        }

        public IActionResult Index()
        {
            return RedirectToAction("PlanContable");
        }

        public async Task<IActionResult> PlanContable()
        {
            List<CuentasContable> cuentas = await this.repo.GetPlanContableAsync();
            return View(cuentas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCuenta(string codigo, string nombre, string tipo)
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();
            CuentasContable cuenta = new CuentasContable
            {
                EmpresaId = idEmpresa,
                Codigo = codigo,
                Nombre = nombre,
                Tipo = tipo
            };

            var resultado = await this.repo.CrearCuentaContableAsync(cuenta);

            if (resultado.exito)
            {
                TempData["EXITO"] = resultado.mensaje;
            }
            else
            {
                TempData["ERROR"] = resultado.mensaje;
            }

            return RedirectToAction("PlanContable"); ;
        }
    }
}
