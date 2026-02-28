using Microsoft.AspNetCore.Mvc;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    public class EmpleadosController : Controller
    {
        private EmpleadosRepository repoEmpleados;
        private DepartamentsRepository repoDepartamentos;

        public EmpleadosController(EmpleadosRepository repoEmpleados, DepartamentsRepository repoDepartamentos)
        {
            this.repoEmpleados = repoEmpleados;
            this.repoDepartamentos = repoDepartamentos;
        }

        public async Task<IActionResult> Index(int? idDepartamento)
        {
            EmpleadosViewModel model = new EmpleadosViewModel();
            model.Departamentos = await this.repoDepartamentos.GetDepartamentosAsync();
            if (idDepartamento != null)
            {
                model.Empleados = await this.repoEmpleados.GetEmpleadosDepartamentoAsync(idDepartamento);
                ViewBag.DepartamentoSeleccionado = idDepartamento.Value;
            }
            else
            {
                model.Empleados = await this.repoEmpleados.GetEmpleadosAsync();
            }
            return View(model);
        }
    }
}
