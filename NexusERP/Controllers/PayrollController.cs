using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Filters;
using NexusERP.Helpers;
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

        [HttpGet]
        public async Task<IActionResult> Calcular(int empleadoId, int mes, int anio)
        {
            Empleado empleado = await this.repoNominas.GetEmpleadoParaNominaAsync(empleadoId);

            if (empleado == null)
            {
                return NotFound("El empleado no existe o no tienes permisos.");
            }

            // 2. Usar tu lógica de negocio para el IRPF
            decimal porcentajeIrpf = CalcularPorcentajeIRPF(
                empleado.SalarioBrutoAnual,
                empleado.NumeroHijos,
                empleado.PorcentajeDiscapacidad,
                empleado.EstadoCivil.ToString()
            );

            // 3. Crear el paquete (ViewModel) para la vista
            CalcularNominaViewModel model = new CalcularNominaViewModel
            {
                EmpleadoId = empleado.Id,
                Mes = mes,
                Anio = anio,
                EmpleadoNombre = $"{empleado.Nombre} {empleado.Apellidos}".Trim(),
                DepartamentoNombre = empleado.Departamento?.Nombre ?? "Sin Asignar",

                // Calculamos el primer y último día de ese mes/año exacto
                FechaInicio = new DateTime(anio, mes, 1),
                FechaFin = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes)),

                PorcentajeIRPF = porcentajeIrpf,
                Conceptos = new List<ConceptoNominaItemViewModel>()
            };

            // Usamos el helper que creamos antes para el circulito
            model.EmpleadoIniciales = model.EmpleadoNombre.ObtenerIniciales();

            // 4. Transformar los Conceptos de la BBDD a Conceptos de la Vista
            if (empleado.ConceptosFijosEmpleados != null)
            {
                foreach (var cf in empleado.ConceptosFijosEmpleados.Where(c => c.Activo == true))
                {
                    model.Conceptos.Add(new ConceptoNominaItemViewModel
                    {
                        ConceptoId = cf.ConceptoId,
                        Nombre = cf.Concepto.Nombre,
                        Tipo = cf.Concepto.Tipo,
                        Importe = cf.ImporteFijo,
                        TributaIRPF = cf.Concepto.TributaIrpf ?? true,

                        // Lógica de negocio visual: Definimos qué conceptos cambian si el empleado trabaja menos días
                        EsProrrateable = EsConceptoProrrateable(cf.Concepto.Codigo)
                    });
                }
            }

            return View(model);
        }

        // --- TUS MÉTODOS PRIVADOS DE LÓGICA DE NEGOCIO ---

        private decimal CalcularPorcentajeIRPF(decimal salarioBrutoAnual, int hijos, int discapacidad, string estadoCivil)
        {
            // 1. Base según salario (Tabla simplificada 2024)
            decimal porcentajeBase = 0;

            if (salarioBrutoAnual < 12450) porcentajeBase = 19;
            else if (salarioBrutoAnual < 20200) porcentajeBase = 24;
            else if (salarioBrutoAnual < 35200) porcentajeBase = 30;
            else if (salarioBrutoAnual < 60000) porcentajeBase = 37;
            else porcentajeBase = 45;

            // 2. Reducciones (La "gracia" del algoritmo)
            porcentajeBase -= (hijos * 2);

            if (discapacidad >= 33 && discapacidad < 65) porcentajeBase -= 5;
            if (discapacidad >= 65) porcentajeBase -= 10;

            // Por Estado Civil: Casados pagan un pelín menos (simplificación)
            if (estadoCivil == "Casado") porcentajeBase -= 1;

            // 3. Límite mínimo
            if (porcentajeBase < 2) return 2;

            return porcentajeBase;
        }

        private bool EsConceptoProrrateable(string codigoConcepto)
        {
            string[] codigosProrrateables = { "SAL_BASE", "PLUS_TRANS", "PLUS_ANTIG", "PLUS_ACT" };

            return codigosProrrateables.Contains(codigoConcepto?.ToUpper());
        }

    }
}
