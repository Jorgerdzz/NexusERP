using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Models;

namespace NexusERP.Repositories
{
    public class AccountingRepository
    {
        private NexusContext context;

        public AccountingRepository(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<CuentasContable>> GetPlanContableAsync()
        {
            return await this.context.CuentasContables
                         .OrderBy(c => c.Codigo)
                         .ToListAsync();
        }

        public async Task<(bool exito, string mensaje)> CrearCuentaContableAsync(CuentasContable cuenta)
        {
            try
            {
                bool existe = await this.context.CuentasContables
                               .AnyAsync(c => c.Codigo == cuenta.Codigo && c.EmpresaId == cuenta.EmpresaId);
                if (existe)
                {
                    return (false, "Ya existe una cuenta registrada con ese código");
                }
                await this.context.CuentasContables.AddAsync(cuenta);
                await this.context.SaveChangesAsync();
                return (true, "Cuenta contable creada con éxito.");
            }
            catch(Exception ex)
            {
                return (false, "Error al crear la cuenta: " + ex.Message);
            }
        }

        public async Task<List<AsientosContable>> GetLibroDiarioAsync()
        {
            return await this.context.AsientosContables
                .Include(a => a.ApuntesContables)
                    .ThenInclude(ap => ap.Cuenta) 
                .OrderByDescending(a => a.Fecha)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<List<ApuntesContable>> GetApuntesPorCuentaAsync(int cuentaId, DateTime desde, DateTime hasta)
        {
            return await this.context.ApuntesContables
                .Include(ap => ap.Asiento)
                .Where(ap => ap.CuentaId == cuentaId && ap.Asiento.Fecha >= desde && ap.Asiento.Fecha <= hasta)
                .OrderBy(ap => ap.Asiento.Fecha)
                .ThenBy(ap => ap.AsientoId)
                .ToListAsync();
        }

        public async Task<decimal> GetSaldoAnteriorAsync(int cuentaId, DateTime desde)
        {
            var apuntesAnteriores = await this.context.ApuntesContables
                .Include(ap => ap.Asiento)
                .Where(ap => ap.CuentaId == cuentaId && ap.Asiento.Fecha < desde)
                .ToListAsync();

            // Sumamos el Debe total menos el Haber total histórico
            return apuntesAnteriores.Sum(ap => ap.Debe.Value - ap.Haber.Value);
        }

        public async Task<List<ApuntesContable>> GetApuntesAnioActualAsync()
        {
            int anioActual = DateTime.Now.Year;
            return await this.context.ApuntesContables
                .Include(ap => ap.Asiento)
                .Include(ap => ap.Cuenta)
                .Where(ap => ap.Asiento.Fecha.Value.Year == anioActual).ToListAsync();
        }

    }
}
