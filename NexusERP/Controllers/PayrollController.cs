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
        private HelperSessionContextAccessor contextAccessor;

        public PayrollController(PayrollRepository repoNominas, HelperSessionContextAccessor contextAccessor)
        {
            this.repoNominas = repoNominas;
            this.contextAccessor = contextAccessor;
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
                SalarioBrutoAnual = empleado.SalarioBrutoAnual,
                SalarioMensualSugerido = empleado.SalarioBrutoAnual / 14,
                FechaInicio = new DateOnly(anio, mes, 1),
                FechaFin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes)),
                PorcentajeIRPF = porcentajeIrpf,
                Conceptos = new List<ConceptoNominaItemViewModel>()
            };

            model.EmpleadoIniciales = model.EmpleadoNombre.ObtenerIniciales();

            if (empleado.ConceptosFijosEmpleados != null)
            {
                foreach (var cf in empleado.ConceptosFijosEmpleados.Where(c => c.Activo == true))
                {
                    switch (cf.Concepto.Codigo?.ToUpper())
                    {
                        case "SAL_BASE": model.SalarioBase = cf.ImporteFijo; break;
                        case "INCENTIVOS": model.Incentivos = cf.ImporteFijo; break;
                        case "PLUS_DED": model.PlusDedicacion = cf.ImporteFijo; break;
                        case "PLUS_ANT": model.PlusAntiguedad = cf.ImporteFijo; break;
                        case "PLUS_ACT": model.PlusActividad = cf.ImporteFijo; break;
                        case "PLUS_NOC": model.PlusNocturnidad = cf.ImporteFijo; break;
                        case "PLUS_RES": model.PlusResponsabilidad = cf.ImporteFijo; break;
                        case "PLUS_CONV": model.PlusConvenio = cf.ImporteFijo; break;
                        case "PLUS_IDIOM": model.PlusIdiomas = cf.ImporteFijo; break;
                        case "H_EXTRA": model.HorasExtraordinarias = cf.ImporteFijo; break;
                        case "H_COMP": model.HorasComplementarias = cf.ImporteFijo; break;
                        case "SAL_ESP": model.SalarioEspecie = cf.ImporteFijo; break;

                        case "IND_SUP": model.IndemnizacionesSuplidos = cf.ImporteFijo; break;
                        case "PREST_SS": model.PrestacionesSS = cf.ImporteFijo; break;
                        case "IND_DESP": model.IndemnizacionesDespido = cf.ImporteFijo; break;
                        case "PLUS_TRANS": model.PlusTransporte = cf.ImporteFijo; break;
                        case "DIETAS": model.Dietas = cf.ImporteFijo; break;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarNomina(CalcularNominaViewModel model)
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();
            Nomina nomina = new Nomina
            {
                EmpresaId = idEmpresa,
                EmpleadoId = model.EmpleadoId,
                Mes = model.Mes,
                Anio = model.Anio,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,

                BaseCotizacionCc = model.BaseCotizacion_CC,
                BaseCotizacionCp = model.BaseCotizacion_CP,
                BaseIrpf = model.BaseIRPF,
                PorcentajeIrpf = model.PorcentajeIRPF,

                TotalDevengado = model.TotalDevengado,
                TotalDeducciones = model.TotalDeducciones,
                LiquidoApercibir = model.LiquidoAPercibir,

                SsEmpresaContingenciasComunes = model.SS_Empresa_ContingenciasComunes,
                SsEmpresaAccidentesTrabajo = model.SS_Empresa_AccidentesTrabajo,
                SsEmpresaDesempleo = model.SS_Empresa_Desempleo,
                SsEmpresaFormacion = model.SS_Empresa_Formacion,
                SsEmpresaFogasa = model.SS_Empresa_Fogasa,
                SsEmpresaTotal = model.SS_Empresa_Total,

                FechaGeneracion = DateTime.Now
            };

            nomina.NominaDetalles = new List<NominaDetalle>();

            void AgregarDetalle(string codigo, string nombre, decimal importe, int tipo)
            {
                if (importe > 0)
                {
                    nomina.NominaDetalles.Add(new NominaDetalle
                    {
                        Codigo = codigo,
                        ConceptoNombre = nombre,
                        Importe = importe,
                        Tipo = tipo 
                    });
                }
            }

            AgregarDetalle("SAL_BASE", "Salario Base", model.SalarioBase, 1);
            AgregarDetalle("INCENTIVOS", "Incentivos", model.Incentivos, 1);
            AgregarDetalle("PLUS_DED", "Plus especial dedicación", model.PlusDedicacion, 1);
            AgregarDetalle("PLUS_ANT", "Plus antigüedad", model.PlusAntiguedad, 1);
            AgregarDetalle("PLUS_ACT", "Plus actividad", model.PlusActividad, 1);
            AgregarDetalle("PLUS_NOC", "Plus nocturnidad", model.PlusNocturnidad, 1);
            AgregarDetalle("PLUS_RES", "Plus responsabilidad", model.PlusResponsabilidad, 1);
            AgregarDetalle("PLUS_CONV", "Plus convenio", model.PlusConvenio, 1);
            AgregarDetalle("PLUS_IDIOM", "Plus idiomas", model.PlusIdiomas, 1);
            AgregarDetalle("H_EXTRA", "Horas extraordinarias", model.HorasExtraordinarias, 1);
            AgregarDetalle("H_COMP", "Horas complementarias", model.HorasComplementarias, 1);
            AgregarDetalle("SAL_ESP", "Salario en especie", model.SalarioEspecie, 1);

            // Devengos No Salariales (Tipo 1)
            AgregarDetalle("IND_SUP", "Indemnizaciones o Suplidos", model.IndemnizacionesSuplidos, 1);
            AgregarDetalle("PREST_SS", "Prestaciones S.S.", model.PrestacionesSS, 1);
            AgregarDetalle("IND_DESP", "Indemnizaciones por despido", model.IndemnizacionesDespido, 1);
            AgregarDetalle("PLUS_TRANS", "Plus transporte", model.PlusTransporte, 1);
            AgregarDetalle("DIETAS", "Dietas", model.Dietas, 1);

            // Deducciones Trabajador (Tipo 2)
            AgregarDetalle("DED_CC", "Contingencias Comunes", model.SS_ContingenciasComunes, 2);
            AgregarDetalle("DED_MEI", "M.E.I.", model.SS_MEI, 2);
            AgregarDetalle("DED_DES", "Desempleo", model.SS_Desempleo, 2);
            AgregarDetalle("DED_FP", "Formación Profesional", model.SS_Formacion, 2);
            AgregarDetalle("DED_IRPF", "Retención I.R.P.F.", model.RetencionIRPF, 2);

            var (exito, mensaje) = await this.repoNominas.GuardarNominaCompletaAsync(nomina);

            if (exito)
            {
                TempData["EXITO"] = mensaje;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ERROR"] = mensaje;
                return RedirectToAction("Calcular", new { empleadoId = model.EmpleadoId, mes = model.Mes, anio = model.Anio });
            }
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
