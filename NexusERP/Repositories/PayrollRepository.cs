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
                .Where(e => e.Activo == true)
                .ToListAsync();
        }

        public async Task<Empleado> GetEmpleadoParaNominaAsync(int empleadoId)
        {
            return await this.context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.ConceptosFijosEmpleados.Where(c => c.Activo == true))
                    .ThenInclude(cf => cf.Concepto) 
                .FirstOrDefaultAsync(e => e.Id == empleadoId && e.Activo == true);
        }

        public async Task<(bool exito, string mensaje)> GuardarNominaCompletaAsync(Nomina nomina)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();

            try
            {

                await this.context.Nominas.AddAsync(nomina);
                await this.context.SaveChangesAsync(); 

                foreach (var detalle in nomina.NominaDetalles)
                {
                    var conceptoCatalogo = await this.context.ConceptosSalariales
                        .FirstOrDefaultAsync(c => c.Codigo == detalle.Codigo && c.EmpresaId == nomina.EmpresaId);

                    if (conceptoCatalogo == null)
                    {
                        throw new Exception($"El concepto salarial '{detalle.Codigo}' no existe en el catálogo de esta empresa. Revise la configuración.");
                    }

                    detalle.ConceptoId = conceptoCatalogo.Id;
                    this.context.NominaDetalles.Update(detalle);

                    if (detalle.Tipo == 1 && detalle.Codigo != "H_EXTRA")
                    {
                        var conceptoFijo = await this.context.ConceptosFijosEmpleados
                            .FirstOrDefaultAsync(cf => cf.EmpleadoId == nomina.EmpleadoId && cf.ConceptoId == conceptoCatalogo.Id);

                        if (conceptoFijo != null)
                        {
                            conceptoFijo.ImporteFijo = detalle.Importe;
                            conceptoFijo.Activo = true;
                            this.context.ConceptosFijosEmpleados.Update(conceptoFijo);
                        }
                        else
                        {
                            ConceptosFijosEmpleado nuevoFijo = new ConceptosFijosEmpleado
                            {
                                EmpresaId = nomina.EmpresaId,
                                EmpleadoId = nomina.EmpleadoId,
                                ConceptoId = conceptoCatalogo.Id,
                                ImporteFijo = detalle.Importe,
                                Activo = true
                            };
                            await this.context.ConceptosFijosEmpleados.AddAsync(nuevoFijo);
                        }
                    }
                }

                await this.context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, "Nómina guardada y conceptos actualizados con éxito.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error al generar la nómina: " + ex.Message);
            }
        }

    }
}
