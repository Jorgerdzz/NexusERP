using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Filters;
using NexusERP.Models;
using NexusERP.Models.UI;
using NexusERP.Repositories;
using NexusERP.Services;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartamentoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                AlertService.Warning(TempData, "Por favor, revisa los campos del formulario.");
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
                AlertService.Success(TempData, $"El departamento '{model.Nombre}' se ha creado correctamente.");
            }
            else
            {
                AlertService.Error(TempData, "Error al crear el departamento");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditDepartamentoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                AlertService.Warning(TempData, "Datos inválidos. No se pudo actualizar el departamento.");
                return RedirectToAction("Index");
            }

            Departamento departamentoActualizado = new Departamento
            {
                Id = model.Id,
                Nombre = model.Nombre,
                PresupuestoAnual = model.PresupuestoAnual
            };

            bool actualizado = await this.repoDepartamentos.UpdateDepartamentoAsync(departamentoActualizado);

            if (actualizado)
            {
                AlertService.Toast(TempData, "Departamento actualizado correctamente", "success");
            }
            else
            {
                AlertService.Error(TempData, "No se pudo actualizar. El departamento no existe.");
            }

            return RedirectToAction("Index");
        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool eliminado = await this.repoDepartamentos.DeleteDepartamentoAsync(id);
            if (eliminado)
            {
                AlertService.Success(TempData, "Departamento eliminado correctamente");
                return Ok();
            }
            else
            {
                return BadRequest("No se puede eliminar el departamento. Asegúrate de que no tenga empleados asignados antes de borrarlo.");
            }
        }

    }
}
