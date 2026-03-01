using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmpleadoViewModel model)
        {
            if (ModelState.IsValid)
            {
                Empleado emp = new Empleado
                {
                    Id = model.DepartamentoId,
                    Nombre = model.Nombre,
                    Apellidos = model.Apellidos,
                    Dni = model.DNI,
                    EmailCorporativo = model.EmailCorporativo,
                    Telefono = model.Telefono,
                    FechaNacimiento = model.FechaNacimiento,
                    NumSeguridadSocial = model.NumSeguridadSocial,
                    FechaAntiguedad = model.FechaAntiguedad,
                    GrupoCotizacion = (int)model.GrupoCotizacion,
                    EstadoCivil = (int)model.EstadoCivil,
                    SalarioBrutoAnual = model.SalarioBrutoAnual,
                    Iban = model.IBAN,
                    NumeroHijos = model.NumeroHijos,
                    PorcentajeDiscapacidad = model.PorcentajeDiscapacidad,
                    Activo = (model.Activo == EstadoEmpleado.Activo)
                };

                bool exito = await this.repoEmpleados.CreateEmpleadoAsync(emp);

                if (exito)
                {
                    TempData["SuccessMessage"] = "Empleado añadido correctamente a la plantilla.";
                    return RedirectToAction("Details", "Departaments", new { id = model.DepartamentoId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocurrió un error inesperado al guardar en la base de datos.";
                }

            }
            else
            {
                TempData["ErrorMessage"] = "Revisa los datos del formulario. Algunos campos no tienen el formato correcto.";
                return RedirectToAction("Details", "Departaments", new { id = model.DepartamentoId });
            }
            return RedirectToAction("Details", "Departaments", new { id = model.DepartamentoId });
        }
    }
}
