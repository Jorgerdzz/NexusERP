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

        public async Task<IndexDepartamentosViewModel> GetDepartamentosModelAsync()
        {
            IndexDepartamentosViewModel model = new IndexDepartamentosViewModel
            {
                TotalDepartamentos = await this.context.Departamentos.CountAsync(),
                TotalEmpleadosGlobal = await this.context.Empleados.CountAsync(),
                PresupuestoTotalGlobalAnual = await this.context.Departamentos.SumAsync(d => (decimal?)d.PresupuestoAnual) ?? 0,
                PresupuestoTotalGlobalMensual = await this.context.Departamentos.SumAsync(d => (decimal?)d.PresupuestoAnual / 12) ?? 0,
                SalarioPromedioGlobalAnual = await this.context.Empleados.AverageAsync(e => (decimal?)e.SalarioBrutoAnual) ?? 0,
                SalarioPromedioGlobalMensual = await this.context.Empleados.AverageAsync(e => (decimal?)e.SalarioBrutoAnual / 12) ?? 0,
                Departamentos = await this.context.Departamentos
                    .Select(d => new DepartamentoCardViewModel
                    {
                        Id = d.Id,
                        Nombre = d.Nombre,
                        PresupuestoAnual = d.PresupuestoAnual,
                        PresupuestoMensual = d.PresupuestoAnual / 12,
                        NumeroEmpleados = d.Empleados.Count(),
                        SalarioPromedio = d.Empleados.Any() ? d.Empleados.Average(e => e.SalarioBrutoAnual) : 0
                    })
                    .ToListAsync()
                    };
            return model;
        }

        public async Task<Departamento> GetDepartamentoAsync(int idDepartamento)
        {
            var consulta = from datos in this.context.Departamentos
                           where datos.Id == idDepartamento
                           select datos;
            return await consulta.FirstOrDefaultAsync();
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
