using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using QuestPDF.Fluent;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
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
                        LiquidoAPercibir = nominaDelMes?.LiquidoApercibir,
                        Estado = nominaDelMes?.Estado ?? "Pendiente"
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
                (EstadoCivil)empleado.EstadoCivil
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
                SsEmpresaMei = model.SS_Empresa_MEI,
                SsEmpresaHorasExtras = model.SS_Empresa_HorasExtras,

                SsEmpresaTotal = model.SS_Empresa_Total,

                Estado = "Pendiente",

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
                return RedirectToAction("Details", new { idNomina = nomina.Id});
            }
            else
            {
                TempData["ERROR"] = mensaje;
                return RedirectToAction("Calcular", new { empleadoId = model.EmpleadoId, mes = model.Mes, anio = model.Anio });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPago(int idNomina)
        {
            bool resultado = await this.repoNominas.PagarNominaAsync(idNomina);

            if (resultado)
            {
                TempData["EXITO"] = "Nómina pagada y contabilizada correctamente.";
            }
            else
            {
                TempData["ERROR"] = "No se ha podido procesar el pago de la nómina.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int idNomina)
        {
            Nomina nomina = await this.repoNominas.GetNominaEmpleado(idNomina);
            return View(nomina);
        }

        public async Task<IActionResult> DescargarPdf(int idNomina)
        {
            Nomina nomina = await this.repoNominas.GetNominaEmpleado(idNomina);

            if (nomina == null) return NotFound();

            // Usamos la magia de QuestPDF
            var document = new NominaDocument(nomina);
            byte[] pdfBytes = document.GeneratePdf();

            string nombreArchivo = $"Nomina_{nomina.Empleado.Nombre}_{nomina.Empleado.Apellidos}_{nomina.Anio}_{nomina.Mes:D2}.pdf";

            return File(pdfBytes, "application/pdf", nombreArchivo);
        }



        private decimal CalcularPorcentajeIRPF(decimal salarioBrutoAnual, int numeroHijos, int porcentajeDiscapacidad, EstadoCivil estadoCivil)
        {
            // 1. Determinar situación familiar
            string situacionFamiliar = "S1"; // Soltero sin hijos por defecto
            bool esCasado = (estadoCivil == EstadoCivil.Casado);

            if (esCasado)
            {
                if (numeroHijos >= 2) situacionFamiliar = "M3";
                else if (numeroHijos == 1) situacionFamiliar = "M2";
                else situacionFamiliar = "M1";
            }
            else
            {
                // Esto aplica a Solteros, Divorciados y Viudos
                if (numeroHijos > 0) situacionFamiliar = "S2";
            }

            // 2. Aplicar reducciones por discapacidad
            decimal reduccionDiscapacidad = 0;
            if (porcentajeDiscapacidad >= 65)
            {
                reduccionDiscapacidad = 4000;
            }
            else if (porcentajeDiscapacidad >= 33)
            {
                reduccionDiscapacidad = 2000;
            }

            // 3. Base de cálculo (salario anual - reducciones)
            decimal baseCalculo = Math.Max(0, salarioBrutoAnual - reduccionDiscapacidad);

            // 4. Determinar porcentaje según tramos y situación familiar
            decimal porcentajeIRPF = 0;

            if (baseCalculo < 12450)
            {
                porcentajeIRPF = situacionFamiliar switch
                {
                    "S1" => 9.5m,
                    "S2" => 7.5m,
                    "M1" => 8.5m,
                    "M2" => 6.5m,
                    "M3" => 5.5m,
                    _ => 9.5m
                };
            }
            else if (baseCalculo < 20200)
            {
                porcentajeIRPF = situacionFamiliar switch
                {
                    "S1" => 12.0m,
                    "S2" => 10.0m,
                    "M1" => 11.0m,
                    "M2" => 9.0m,
                    "M3" => 8.0m,
                    _ => 12.0m
                };
            }
            else if (baseCalculo < 35200)
            {
                porcentajeIRPF = situacionFamiliar switch
                {
                    "S1" => 15.0m,
                    "S2" => 13.0m,
                    "M1" => 14.0m,
                    "M2" => 12.0m,
                    "M3" => 11.0m,
                    _ => 15.0m
                };
            }
            else if (baseCalculo < 60000)
            {
                porcentajeIRPF = situacionFamiliar switch
                {
                    "S1" => 18.5m,
                    "S2" => 16.5m,
                    "M1" => 17.5m,
                    "M2" => 15.5m,
                    "M3" => 14.5m,
                    _ => 18.5m
                };
            }
            else
            {
                porcentajeIRPF = situacionFamiliar switch
                {
                    "S1" => 22.5m,
                    "S2" => 20.5m,
                    "M1" => 21.5m,
                    "M2" => 19.5m,
                    "M3" => 18.5m,
                    _ => 22.5m
                };
            }

            // 5. Ajuste mínimo del 2% y máximo del 40%
            return Math.Min(40m, Math.Max(2m, porcentajeIRPF));
        }

    }
}
