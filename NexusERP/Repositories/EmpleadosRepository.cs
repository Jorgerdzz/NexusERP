using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.ViewModels;

namespace NexusERP.Repositories
{
    public class EmpleadosRepository
    {
        private NexusContext context;
        private HelperSessionContextAccessor contextAccessor;

        public EmpleadosRepository(NexusContext context, HelperSessionContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            return await this.context.Empleados.ToListAsync();
        }

        public async Task<List<Empleado>> GetEmpleadosDepartamentoAsync(int? idDepartamento)
        {
            var consulta = from datos in this.context.Empleados
                           where datos.DepartamentoId == idDepartamento
                           select datos;
            return await consulta.ToListAsync();
        }

    }
}
