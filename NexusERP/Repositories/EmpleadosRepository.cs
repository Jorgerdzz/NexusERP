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

        public async Task<int> GetTotalEmpleadosAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<decimal> GetSalarioPromedioAnualAsync()
        {
            return await this.context.Empleados.AverageAsync(e => (decimal?)e.SalarioBrutoAnual) ?? 0;
        }

        public async Task<bool> CreateEmpleadoAsync(Empleado Empleado)
        {
            try
            {
                Empleado.EmpresaId = this.contextAccessor.GetEmpresaIdSession();
                await this.context.Empleados.AddAsync(Empleado);
                await this.context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                string errorPrincipal = ex.Message;

                // ¡EL VERDADERO ERROR DE SQL SERVER ESTÁ AQUÍ!
                string errorRealDeBaseDeDatos = ex.InnerException != null ? ex.InnerException.Message : "No hay detalle interno";

                // Pon un punto de interrupción (BreakPoint) en la línea del return false;
                return false;
            }
        }

    }
}
