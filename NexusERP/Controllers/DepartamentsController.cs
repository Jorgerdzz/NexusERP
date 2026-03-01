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
            int totalDepartamentos = await this.repoDepartamentos.GetTotalDepartamentosAsync();
            int totalEmpleados = await this.repoEmpleados.GetTotalEmpleadosAsync();
            decimal presupuestoAnual = await this.repoDepartamentos.GetPresupuestoTotalAnualAsync();
            decimal salarioMedioAnual = await this.repoEmpleados.GetSalarioPromedioAnualAsync();

            var estadisticas = await this.repoDepartamentos.GetEstadisticasDepartamentosAsync();

            IndexDepartamentosViewModel model = new IndexDepartamentosViewModel
            {
                TotalDepartamentos = totalDepartamentos,
                TotalEmpleadosGlobal = totalEmpleados,
                PresupuestoTotalGlobalAnual = presupuestoAnual,
                PresupuestoTotalGlobalMensual = presupuestoAnual / 12,
                SalarioPromedioGlobalAnual = salarioMedioAnual,
                SalarioPromedioGlobalMensual = salarioMedioAnual / 12,

                Departamentos = estadisticas.Select(e => new DepartamentoCardViewModel
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    PresupuestoAnual = e.PresupuestoAnual,
                    PresupuestoMensual = e.PresupuestoAnual / 12,
                    NumeroEmpleados = e.NumeroEmpleados, 
                    SalarioPromedio = e.SalarioPromedio
                }).ToList()
            };
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            Departamento dep = await this.repoDepartamentos.GetDepartamentoAsync(id);
            if (dep == null) return NotFound();
            List<Empleado> empleados = await this.repoEmpleados.GetEmpleadosDepartamentoAsync(id);
            DepartamentoDetailsViewModel model = new DepartamentoDetailsViewModel
            {
                Id = dep.Id,
                Nombre = dep.Nombre,
                PresupuestoAnual = dep.PresupuestoAnual,
                PresupuestoMensual = dep.PresupuestoAnual / 12,
                NumeroEmpleados = empleados.Count(),
                Empleados = empleados,
                SalarioPromedioAnual = empleados.Any() ? empleados.Average(e => e.SalarioBrutoAnual) : 0,
                SalarioPromedioMensual = empleados.Any() ? empleados.Average(e => e.SalarioBrutoAnual) / 12 : 0
            };
            return View(model);
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
