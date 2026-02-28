using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Filters;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [AuthorizeUser(Rol = RolesUsuario.Admin)]
    public class DepartamentsController : Controller
    {
        private DepartamentsRepository repo;

        public DepartamentsController(DepartamentsRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            IndexDepartamentosViewModel departamentos = await this.repo.GetDepartamentosAsync();
            return View(departamentos);
        }

        public async Task<IActionResult> Details(int id)
        {
            Departamento dep = await this.repo.GetDepartamentoAsync(id);
            return View(dep);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartamentoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            Departamento nuevoDepartamento = new Departamento
            {
                Nombre = model.Nombre,
                PresupuestoMensual = model.PresupuestoMensual
            };
            bool creado = await this.repo.CreateDepartamentoAsync(nuevoDepartamento);
            if (creado)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

    }
}
