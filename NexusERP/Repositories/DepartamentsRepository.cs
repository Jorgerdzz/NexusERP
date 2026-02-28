using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.ViewModels;

namespace NexusERP.Repositories
{
    public class DepartamentsRepository
    {
        private NexusContext context;
        private HelperSessionContextAccessor contextAccessor;

        public DepartamentsRepository(NexusContext context, HelperSessionContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task<List<Departamento>> GetDepartamentosAsync()
        {
            return await this.context.Departamentos.ToListAsync();
        }

        public async Task<Departamento> GetDepartamentoAsync(int idDepartamento)
        {
            var consulta = from datos in this.context.Departamentos
                           where datos.Id == idDepartamento
                           select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalDepartamentosAsync()
        {
            return await this.context.Departamentos.CountAsync();
        }

        public async Task<decimal> GetPresupuestoTotalAnualAsync()
        {
            return await this.context.Departamentos.SumAsync(d => (decimal?)d.PresupuestoAnual) ?? 0;
        }

        public async Task<List<(int Id, string Nombre, decimal PresupuestoAnual, int NumeroEmpleados, decimal SalarioPromedio)>> GetEstadisticasDepartamentosAsync()
        {
            var consulta = await this.context.Departamentos
                .Select(d => new
                {
                    d.Id,
                    d.Nombre,
                    d.PresupuestoAnual,
                    NumeroEmpleados = d.Empleados.Count(),
                    SalarioPromedio = d.Empleados.Any() ? d.Empleados.Average(e => e.SalarioBrutoAnual) : 0
                })
                .ToListAsync();
            return consulta.Select(c => (c.Id, c.Nombre, c.PresupuestoAnual, c.NumeroEmpleados, c.SalarioPromedio)).ToList();
        }

        public async Task<bool> CreateDepartamentoAsync(Departamento departamento)
        {
            try
            {
                departamento.EmpresaId = this.contextAccessor.GetEmpresaIdSession();
                await this.context.Departamentos.AddAsync(departamento);
                await this.context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDepartamentoAsync(Departamento departamento)
        {
            try
            {
                var original = await this.GetDepartamentoAsync(departamento.Id);

                if (original == null) return false;

                original.Nombre = departamento.Nombre;
                original.PresupuestoAnual = departamento.PresupuestoAnual;

                await this.context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
