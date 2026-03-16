using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using Microsoft.AspNetCore.Authorization;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using QuestPDF.Fluent;
using System.Threading.Tasks;
using NexusERP.Services;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
    public class FacturacionController : Controller
    {
        private ClienteRepository repoClientes;
        private FacturacionRepository repoFacturas;
        private HelperSessionContextAccessor contextAccessor;

        public FacturacionController(ClienteRepository repoClientes, FacturacionRepository repoFacturas, HelperSessionContextAccessor contextAccessor)
        {
            this.repoClientes = repoClientes;
            this.repoFacturas = repoFacturas;
            this.contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();

            DashboardFacturacionViewModel model = new DashboardFacturacionViewModel();

            List<Cliente> clientes = await this.repoClientes.GetClientesAsync();
            List<Factura> facturas = await this.repoFacturas.GetFacturasAsync();

            model.TotalClientes = clientes.Count(c => c.Activo);
            model.TotalFacturas = facturas.Count;

            model.FacturadoEsteMes = facturas.Where(f => f.FechaEmision.Month == DateTime.Now.Month && f.FechaEmision.Year == DateTime.Now.Year)
                .Sum(f => f.TotalFactura);

            model.PendienteDeCobro = facturas.Where(f => f.Estado == "Pendiente")
                .Sum(f => f.TotalFactura);

            model.UltimasFacturas = facturas.Take(5).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int idFactura)
        {
            Factura factura = await this.repoFacturas.GetFacturaAsync(idFactura);
            return View(factura);
        }

        public async Task<IActionResult> Clientes()
        {
            List<Cliente> clientes = await this.repoClientes.GetClientesAsync();
            return View(clientes);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCliente(string razonSocial, string cifNif, string email)
        {
            int empresaId = this.contextAccessor.GetEmpresaIdSession();

            Cliente nuevo = new Cliente
            {
                EmpresaId = empresaId,
                RazonSocial = razonSocial,
                CifNif = cifNif,
                Email = email,
                Activo = true
            };

            bool creado = await this.repoClientes.CrearClienteAsync(nuevo);

            if (creado)
            {
                AlertService.Toast(TempData, "Cliente guardado correctamente", "success");
            }
            else
            {
                AlertService.Error(TempData, "Hubo un error al guardar el cliente");
            }

            return RedirectToAction("Clientes");
        }

        public async Task<IActionResult> NuevaFactura()
        {
            ViewBag.Clientes = await this.repoClientes.GetClientesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuardarFactura([FromBody] NuevaFacturaViewModel model)
        {
            int empresaId = this.contextAccessor.GetEmpresaIdSession();

            decimal baseImponible = model.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);
            decimal ivaTotal = baseImponible * (model.PorcentajeIva / 100m);
            decimal totalFactura = baseImponible + ivaTotal;

            // 2. Montar la entidad Factura
            Factura factura = new Factura
            {
                EmpresaId = empresaId,
                ClienteId = model.ClienteId,
                NumeroFactura = model.NumeroFactura,
                FechaEmision = model.FechaEmision,
                BaseImponible = baseImponible,
                IvaTotal = ivaTotal,
                TotalFactura = totalFactura,
                Estado = "Pendiente",
                EsEmitida = true,
                FacturaDetalles = new List<FacturaDetalle>()
            };

            // 3. Montar los detalles
            foreach (var linea in model.Lineas)
            {
                factura.FacturaDetalles.Add(new FacturaDetalle
                {
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = linea.PrecioUnitario,
                    TotalLinea = linea.Cantidad * linea.PrecioUnitario
                });
            }

            // 4. Magia de Guardado y Contabilización
            var res = await this.repoFacturas.GuardarFacturaYContabilizarAsync(factura, empresaId);

            if (res.exito)
            {
                AlertService.Toast(TempData, "Facutra emitida correctamente", "success");
                return Ok();
            }
            else
            {
                AlertService.Error(TempData, "Hubo un error al emitir la factura");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCobro(int idFactura)
        {
            bool resultado = await this.repoFacturas.CobrarFacturaAsync(idFactura);
            if (resultado)
            {
                AlertService.Success(TempData, "Facutra cobrada correctamente.");
            }
            else
            {
                AlertService.Error(TempData, "No se ha podido cobrar la factura.");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DescargarPdf(int idFactura)
        {
            var factura = await this.repoFacturas.GetFacturaAsync(idFactura);
            if (factura == null) return NotFound();

            var document = new FacturaDocument(factura);
            byte[] pdfBytes = document.GeneratePdf();

            string nombreArchivo = $"Factura_{factura.NumeroFactura}.pdf";

            return File(pdfBytes, "application/pdf", nombreArchivo);
        }

    }

}
