using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Helpers;
using NexusERP.Models;

namespace NexusERP.Repositories
{
    public class PayrollRepository
    {
        private NexusContext context;
        private HelperSessionContextAccessor contextAccessor;

        public PayrollRepository(NexusContext context, HelperSessionContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task<List<Empleado>> GetEmpleadosConNominasAsync(int mes, int anio)
        {
            return await this.context.Empleados
                .Include(e => e.Departamento) 
                .Include(e => e.Nominas.Where(n => n.Mes == mes && n.Anio == anio)) 
                .ToListAsync();
        }

        public async Task<Empleado> GetEmpleadoParaNominaAsync(int empleadoId)
        {
            return await this.context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.ConceptosFijosEmpleados) 
                    .ThenInclude(cf => cf.Concepto)     
                .FirstOrDefaultAsync(e => e.Id == empleadoId);
        }

    }
}
