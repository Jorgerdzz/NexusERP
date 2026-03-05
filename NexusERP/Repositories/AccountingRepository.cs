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


    }
}
