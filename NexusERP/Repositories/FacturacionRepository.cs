using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Helpers;
using NexusERP.Models;
using System.Threading.Tasks;

namespace NexusERP.Repositories
{
    public class FacturacionRepository
    {
        private NexusContext context;
        private HelperSessionContextAccessor contextAccessor;

        public FacturacionRepository(NexusContext context, HelperSessionContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task<List<Factura>> GetFacturasAsync()
        {
            return await this.context.Facturas.Include(f => f.Cliente).OrderByDescending(f => f.FechaEmision).ToListAsync();
        }

        public async Task<Factura> GetFacturaAsync(int idFactura)
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();

            return await this.context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Empresa)
                .Include(f => f.FacturaDetalles)
                .FirstOrDefaultAsync(f => f.Id == idFactura && f.EmpresaId == idEmpresa);
        }

        public async Task<(bool exito, string mensaje)> GuardarFacturaYContabilizarAsync(Factura factura, int empresaId)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                // 1. OBTENER LAS CUENTAS MAESTRAS DE VENTAS
                var c430 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == "4300000");
                var c700 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == "7000000");
                var c477 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == "4770000");

                if (c430 == null || c700 == null || c477 == null)
                    throw new Exception("Faltan cuentas maestras (430, 700 o 477) en el Plan Contable.");

                // 2. CREAR EL ASIENTO CONTABLE
                var cliente = await this.context.Clientes.FindAsync(factura.ClienteId);

                AsientosContable asiento = new AsientosContable
                {
                    EmpresaId = empresaId,
                    Fecha = factura.FechaEmision,
                    Glosa = $"Factura {factura.NumeroFactura} - {cliente.RazonSocial}"
                };

                await this.context.AsientosContables.AddAsync(asiento);
                await this.context.SaveChangesAsync();

                // 3. CREAR LOS APUNTES DEL ASIENTO (Magia Matemática)
                // DEBE: Cliente (El total que nos debe)
                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asiento.Id, CuentaId = c430.Id, Debe = factura.TotalFactura, Haber = 0 });
                // HABER: Ventas (Nuestra base imponible, ingreso real)
                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asiento.Id, CuentaId = c700.Id, Debe = 0, Haber = factura.BaseImponible });
                // HABER: IVA Repercutido (Lo que debemos a Hacienda)
                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asiento.Id, CuentaId = c477.Id, Debe = 0, Haber = factura.IvaTotal });

                await this.context.SaveChangesAsync();

                // 4. GUARDAR LA FACTURA ENLAZADA AL ASIENTO
                factura.AsientoId = asiento.Id;
                await this.context.Facturas.AddAsync(factura);
                await this.context.SaveChangesAsync();

                // Confirmar todo
                await transaction.CommitAsync();
                return (true, "Factura emitida y contabilizada con éxito.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error contable/facturación: " + ex.Message);
            }
        }

        public async Task<bool> CobrarFacturaAsync(int idFactura)
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();

            using var transaction = await this.context.Database.BeginTransactionAsync();

            try
            {
                Factura factura = await this.context.Facturas
                    .Include(f => f.Cliente)
                    .FirstOrDefaultAsync(f => f.Id == idFactura && f.EmpresaId == idEmpresa);
                if (factura == null) return false;
                if (factura.Estado == "Pagada") return false;

                CuentasContable c572 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == idEmpresa && c.Codigo == "5720000");
                CuentasContable c430 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == idEmpresa && c.Codigo == "4300000");

                AsientosContable asientoCobro = new AsientosContable
                {
                    EmpresaId = idEmpresa,
                    Fecha = DateTime.Now,
                    Glosa = $"Cobro Factura {factura.NumeroFactura} - {factura.Cliente.RazonSocial}"
                };

                await this.context.AsientosContables.AddAsync(asientoCobro);
                await this.context.SaveChangesAsync();

                ApuntesContable cuenta572 = new ApuntesContable
                {
                    AsientoId = asientoCobro.Id,
                    CuentaId = c572.Id,
                    Debe = factura.TotalFactura,
                    Haber = 0
                };

                await this.context.ApuntesContables.AddAsync(cuenta572);

                ApuntesContable cuenta430 = new ApuntesContable
                {
                    AsientoId = asientoCobro.Id,
                    CuentaId = c430.Id,
                    Debe = 0,
                    Haber = factura.TotalFactura
                };

                await this.context.ApuntesContables.AddAsync(cuenta430);

                factura.Estado = "Pagada";

                this.context.Facturas.Update(factura);

                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;


            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }

        }

    }

}
