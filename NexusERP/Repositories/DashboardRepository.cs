using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.DTOs;
using NexusERP.Helpers;
using System.Threading.Tasks;

namespace NexusERP.Repositories
{
    public class DashboardRepository
    {
        private NexusContext context;
        private HelperSessionContextAccessor contextAccessor;

        public DashboardRepository(NexusContext context, HelperSessionContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task<DashboardMetricsDto> GetEstadisticasAsync(int anio)
        {
            DashboardMetricsDto dto = new DashboardMetricsDto();

            dto.TieneDepartamentos = await this.context.Departamentos.AnyAsync();
            dto.TieneClientes = await this.context.Clientes.AnyAsync();
            dto.TieneEmpleados = await this.context.Empleados.AnyAsync();

            dto.TotalFacturadoAnual = await this.context.Facturas
                .Where(f => f.FechaEmision.Year == anio)
                .SumAsync(f => f.TotalFactura);

            dto.TotalGastoSalarial = await this.context.ControlGastos
                .Where(c => c.Anio == anio)
                .SumAsync(c => c.ImporteGasto);

            dto.FacturasPendientes = await this.context.Facturas
                .CountAsync(f => f.Estado == "Pendiente");

            return dto;
        }

    }
}
