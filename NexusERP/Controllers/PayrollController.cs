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
    public class PayrollController : Controller
    {
        private PayrollRepository repoNominas;

        public PayrollController(PayrollRepository repoNominas)
        {
            this.repoNominas = repoNominas;
        }

        public async Task<IActionResult> Index(int? mes, int? anio)
        {
            int mesFiltro = mes ?? DateTime.Now.Month;
            int anioFiltro = anio ?? DateTime.Now.Year;

            List<Empleado> empleados = await this.repoNominas.GetEmpleadosConNominasAsync(mesFiltro, anioFiltro);

            NominasIndexViewModel model = new NominasIndexViewModel
            {
                MesSeleccionado = mesFiltro,
                AnoSeleccionado = anioFiltro,
                Empleados = new List<EmpleadoNominaRowViewModel>()
            };

            if(empleados != null)
            {
                foreach(Empleado emp in empleados)
                {
                    Nomina nominaDelMes = emp.Nominas?.FirstOrDefault();

                    EmpleadoNominaRowViewModel empNomina = new EmpleadoNominaRowViewModel
                    {
                        EmpleadoId = emp.Id,
                        NombreCompleto = $"{emp.Nombre} {emp.Apellidos}".Trim(),
                        Email = emp.EmailCorporativo,
                        DepartamentoNombre = emp.Departamento?.Nombre ?? "Sin Asignar",
                        SalarioBrutoAnual = emp.SalarioBrutoAnual,
                        EstaCalculada = nominaDelMes != null,
                        NominaId = nominaDelMes?.Id,
                        LiquidoAPercibir = nominaDelMes?.LiquidoApercibir
                    };
                    model.Empleados.Add(empNomina);
                }
            }

            return View(model);
        }
    }
}
