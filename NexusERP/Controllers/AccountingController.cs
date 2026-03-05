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
    public class AccountingController : Controller
    {
        private AccountingRepository repo;
        private HelperSessionContextAccessor contextAccessor;

        public AccountingController(AccountingRepository repo, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
        }

        public IActionResult Index()
        {
            return RedirectToAction("PlanContable");
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
                TempData["EXITO"] = resultado.mensaje;
            }
            else
            {
                TempData["ERROR"] = resultado.mensaje;
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
                    Origen = a.Glosa.Contains("Nómina") ? "Nómina" : "Manual" // Pequeño truco visual
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

                        modelo.Movimientos.Add(new NexusERP.ViewModels.MovimientoMayorViewModel
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
