using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.DTOs;
using NexusERP.Helpers;
using NexusERP.Models;
using System.Threading.Tasks;

namespace NexusERP.Repositories
{
    public class ReportsRepository
    {
        private NexusContext context;
        private HelperSessionContextAccessor contextAccessor;

        public ReportsRepository(NexusContext context, HelperSessionContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task<List<ReporteMensualDto>> GetIngresosPorMesAsync(int? anio)
        {
            return await this.context.Facturas
                .Where(f => f.FechaEmision.Year == anio)
                .GroupBy(f => f.FechaEmision.Month)
                .Select(g => new ReporteMensualDto
                {
                    Mes = g.Key,
                    Total = g.Sum(f => f.TotalFactura)
                })
                .ToListAsync();
        }

        public async Task<List<ReporteMensualDto>> GetGastosPorMesAsync(int? anio)
        {
            return await this.context.ControlGastos
                .Where(c => c.Anio == anio)
                .GroupBy(c => c.Mes)
                .Select(g => new ReporteMensualDto
                {
                    Mes = g.Key,
                    Total = g.Sum(c => c.ImporteGasto)
                })
                .ToListAsync();
        }

        public async Task<List<ReporteDepartamentoDto>> GetCostesPorDepartamentoAsync(int? anio)
        {
            return await this.context.ControlGastos
                .Include(c => c.Departamento)
                .Where(c => c.Anio == anio)
                .GroupBy(c => c.Departamento.Nombre)
                .Select(g => new ReporteDepartamentoDto
                {
                    Departamento = g.Key,
                    Total = g.Sum(c => c.ImporteGasto)
                })
                .ToListAsync();
        }



    }
}
