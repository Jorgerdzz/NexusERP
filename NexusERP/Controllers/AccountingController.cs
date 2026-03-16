using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.Services;
using NexusERP.ViewModels;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
    public class AccountingController : Controller
    {
        private AccountingRepository repo;
        private HelperSessionContextAccessor contextAccessor;

        public AccountingController(AccountingRepository repo, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            DashboardFinancieroViewModel modelo = new DashboardFinancieroViewModel();
            List<ApuntesContable> apuntesAnio = await this.repo.GetApuntesAnioActualAsync();

            Dictionary<string,decimal> agrupacionGastos = new Dictionary<string, decimal>();

            foreach (var apunte in apuntesAnio)
            {
                int mesIndex = apunte.Asiento.Fecha.Value.Month - 1; // 0 = Ene, 1 = Feb...

                // REGLAS CONTABLES:
                // Grupo 6 (Gastos): Nacen y crecen por el DEBE.
                if (apunte.Cuenta.Codigo.StartsWith("6"))
                {
                    decimal importeGasto = apunte.Debe.Value - apunte.Haber.Value;
                    if (importeGasto > 0)
                    {
                        modelo.TotalGastos += importeGasto;
                        modelo.GastosMensuales[mesIndex] += importeGasto;

                        
                        if (!agrupacionGastos.ContainsKey(apunte.Cuenta.Nombre))
                            agrupacionGastos[apunte.Cuenta.Nombre] = 0;

                        agrupacionGastos[apunte.Cuenta.Nombre] += importeGasto;
                    }
                }
                // Grupo 7 (Ingresos): Nacen y crecen por el HABER.
                else if (apunte.Cuenta.Codigo.StartsWith("7"))
                {
                    decimal importeIngreso = apunte.Haber.Value - apunte.Debe.Value;
                    if (importeIngreso > 0)
                    {
                        modelo.TotalIngresos += importeIngreso;
                        modelo.IngresosMensuales[mesIndex] += importeIngreso;
                    }
                }
            }

            // Volcamos el diccionario agrupado a las listas del ViewModel
            foreach (var item in agrupacionGastos)
            {
                modelo.NombresCuentasGasto.Add(item.Key);
                modelo.ImportesGasto.Add(item.Value);
            }

            return View(modelo);

        }

        public async Task<IActionResult> PlanContable()
        {
            List<CuentasContable> cuentas = await this.repo.GetPlanContableAsync();
            return View(cuentas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCuenta(string codigo, string nombre, string tipo)
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();
            CuentasContable cuenta = new CuentasContable
            {
                EmpresaId = idEmpresa,
                Codigo = codigo,
                Nombre = nombre,
                Tipo = tipo
            };

            var resultado = await this.repo.CrearCuentaContableAsync(cuenta);

            if (resultado.exito)
            {
                AlertService.Toast(TempData, "Cuenta creada correctamente.");
            }
            else
            {
                AlertService.Error(TempData, "Error al registrar la cuenta contable.");
            }

            return RedirectToAction("PlanContable"); ;
        }

        public async Task<IActionResult> Diario()
        {
            List<AsientosContable> asientos = await this.repo.GetLibroDiarioAsync();
            List<AsientoViewModel> modelo = new List<AsientoViewModel>();

            foreach (AsientosContable a in asientos)
            {
                AsientoViewModel asiento = new AsientoViewModel
                {
                    Id = a.Id,
                    NumeroAsiento = $"AS-{a.Id:D4}", 
                    Fecha = a.Fecha.Value,
                    Glosa = a.Glosa,
                    Origen = a.Glosa.Contains("Nómina") ? "Nómina" : "Factura" 
                };

                foreach (var apunte in a.ApuntesContables)
                {
                    asiento.Apuntes.Add(new ApunteViewModel
                    {
                        CuentaCodigo = apunte.Cuenta.Codigo,
                        CuentaNombre = apunte.Cuenta.Nombre,
                        Debe = apunte.Debe.Value,
                        Haber = apunte.Haber.Value
                    });

                    // Vamos sumando los totales
                    asiento.TotalDebe += apunte.Debe.Value;
                    asiento.TotalHaber += apunte.Haber.Value;
                }

                modelo.Add(asiento);
            }

            return View(modelo);
        }

        // --- FASE 4: LIBRO MAYOR ---
        [HttpGet]
        public async Task<IActionResult> Mayor(int? CuentaIdSeleccionada, DateTime? FechaDesde, DateTime? FechaHasta)
        {
            // Por defecto, mostramos el mes actual si no hay fechas
            DateTime desde = FechaDesde ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime hasta = FechaHasta ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

            var modelo = new NexusERP.ViewModels.MayorViewModel
            {
                FechaDesde = desde,
                FechaHasta = hasta,
                CuentaIdSeleccionada = CuentaIdSeleccionada
            };

            // 1. Llenar el desplegable solo con cuentas de detalle (que tengan 3 o más dígitos)
            var cuentasEmpresa = await this.repo.GetPlanContableAsync();
            modelo.CuentasDisponibles = cuentasEmpresa
                .Where(c => c.Codigo.Length >= 3)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Codigo} - {c.Nombre}"
                }).ToList();

            // 2. Si el usuario ha seleccionado una cuenta, calculamos el Mayor
            if (CuentaIdSeleccionada.HasValue)
            {
                var cuentaDB = cuentasEmpresa.FirstOrDefault(c => c.Id == CuentaIdSeleccionada.Value);
                if (cuentaDB != null)
                {
                    modelo.NombreCuentaSeleccionada = $"{cuentaDB.Codigo} - {cuentaDB.Nombre}";

                    // ¿Cómo suma esta cuenta? (Regla de oro contable)
                    bool sumaPorElDebe = cuentaDB.Tipo == "Activo" || cuentaDB.Tipo == "Gasto";

                    // A. Calcular Saldo Inicial
                    decimal saldoAnteriorBase = await this.repo.GetSaldoAnteriorAsync(CuentaIdSeleccionada.Value, desde);
                    decimal saldoAcumulado = sumaPorElDebe ? saldoAnteriorBase : -saldoAnteriorBase;

                    modelo.Movimientos.Add(new NexusERP.ViewModels.MovimientoMayorViewModel
                    {
                        Fecha = desde,
                        AsientoNumero = "-",
                        Concepto = "Saldo Inicial",
                        Debe = 0,
                        Haber = 0,
                        SaldoAcumulado = saldoAcumulado,
                        EsSaldoInicial = true
                    });

                    // B. Obtener movimientos del periodo seleccionado
                    var apuntes = await this.repo.GetApuntesPorCuentaAsync(CuentaIdSeleccionada.Value, desde, hasta);

                    foreach (var ap in apuntes)
                    {
                        // Actualizar el saldo cronológicamente fila a fila
                        if (sumaPorElDebe)
                            saldoAcumulado += (ap.Debe.Value - ap.Haber.Value);
                        else
                            saldoAcumulado += (ap.Haber.Value - ap.Debe.Value);

                        modelo.Movimientos.Add(new MovimientoMayorViewModel
                        {
                            Fecha = ap.Asiento.Fecha.Value,
                            AsientoNumero = $"AS-{ap.Asiento.Id:D4}",
                            Concepto = ap.Asiento.Glosa,
                            Debe = ap.Debe.Value,
                            Haber = ap.Haber.Value,
                            SaldoAcumulado = saldoAcumulado,
                            EsSaldoInicial = false
                        });
                    }

                    modelo.SaldoFinal = saldoAcumulado;
                }
            }

            return View(modelo);
        }
    }
}
