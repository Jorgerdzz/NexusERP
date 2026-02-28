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
        private DepartamentsRepository repoDepartamentos;
        private EmpleadosRepository repoEmpleados;

        public DepartamentsController(DepartamentsRepository repoDepartamentos, EmpleadosRepository repoEmpleados)
        {
            this.repoDepartamentos = repoDepartamentos;
            this.repoEmpleados = repoEmpleados;
        }

        public async Task<IActionResult> Index()
        {
            IndexDepartamentosViewModel model = await this.repoDepartamentos.GetDepartamentosModelAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            Departamento dep = await this.repoDepartamentos.GetDepartamentoAsync(id);
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
                PresupuestoAnual = model.PresupuestoAnual
            };
            bool creado = await this.repoDepartamentos.CreateDepartamentoAsync(nuevoDepartamento);
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
